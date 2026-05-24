import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { UptimeService } from '../../core/uptime.service';
import { MonitoredService, CreateServiceRequest, TestUrlResult } from '../../core/models';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatSlideToggleModule,
    MatProgressSpinnerModule, MatSnackBarModule, MatTooltipModule
  ],
  templateUrl: './settings.html',
  styleUrl: './settings.scss'
})
export class SettingsComponent implements OnInit {
  private uptimeSvc = inject(UptimeService);
  private snack = inject(MatSnackBar);

  services = signal<MonitoredService[]>([]);
  loading = signal(true);
  editingId = signal<number | null>(null);
  form = signal<CreateServiceRequest>(this.blank());
  testResult = signal<TestUrlResult | null>(null);
  testing = signal(false);

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.uptimeSvc.getServices().subscribe({
      next: s => { this.services.set(s); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  startEdit(s: MonitoredService) {
    this.editingId.set(s.id);
    this.form.set({ name: s.name, url: s.url, isActive: s.isActive, ignoreSslErrors: s.ignoreSslErrors, intervalMinutes: s.intervalMinutes });
  }

  startAdd() {
    this.editingId.set(-1);
    this.form.set(this.blank());
  }

  cancel() { this.editingId.set(null); }

  save() {
    const id = this.editingId();
    if (id === -1) {
      this.uptimeSvc.createService(this.form()).subscribe({
        next: () => { this.snack.open('Service added', '', { duration: 2000 }); this.editingId.set(null); this.load(); },
        error: () => this.snack.open('Error saving', '', { duration: 2000 })
      });
    } else if (id !== null) {
      this.uptimeSvc.updateService(id, this.form()).subscribe({
        next: () => { this.snack.open('Service updated', '', { duration: 2000 }); this.editingId.set(null); this.load(); },
        error: () => this.snack.open('Error saving', '', { duration: 2000 })
      });
    }
  }

  delete(id: number) {
    if (!confirm('Delete this service and all its history?')) return;
    this.uptimeSvc.deleteService(id).subscribe({
      next: () => { this.snack.open('Deleted', '', { duration: 2000 }); this.load(); },
      error: () => this.snack.open('Error deleting', '', { duration: 2000 })
    });
  }

  updateForm(patch: Partial<CreateServiceRequest>) {
    this.form.update(f => ({ ...f, ...patch }));
    if (patch.url !== undefined || patch.ignoreSslErrors !== undefined) {
      this.testResult.set(null);
    }
  }

  testUrl() {
    const { url, ignoreSslErrors } = this.form();
    if (!url) return;
    this.testing.set(true);
    this.testResult.set(null);
    this.uptimeSvc.testUrl(url, ignoreSslErrors).subscribe({
      next: r => { this.testResult.set(r); this.testing.set(false); },
      error: () => { this.testing.set(false); this.snack.open('Test request failed', '', { duration: 2000 }); }
    });
  }

  private blank(): CreateServiceRequest {
    return { name: '', url: '', isActive: true, ignoreSslErrors: false, intervalMinutes: 5 };
  }
}
