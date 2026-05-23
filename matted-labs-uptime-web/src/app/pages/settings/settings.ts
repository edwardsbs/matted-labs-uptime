import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, NgForm } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { UptimeService } from '../../core/uptime.service';
import { MonitoredService, CreateServiceRequest } from '../../core/models';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatSlideToggleModule,
    MatDialogModule, MatProgressSpinnerModule, MatSnackBarModule
  ],
  templateUrl: './settings.html',
  styleUrl: './settings.scss'
})
export class SettingsComponent implements OnInit {
  private uptimeSvc = inject(UptimeService);
  private snack = inject(MatSnackBar);

  services: MonitoredService[] = [];
  loading = true;
  editingId: number | null = null;

  form: CreateServiceRequest = this.blank();

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.uptimeSvc.getServices().subscribe({
      next: s => { this.services = s; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  startEdit(s: MonitoredService) {
    this.editingId = s.id;
    this.form = { name: s.name, url: s.url, isActive: s.isActive, ignoreSslErrors: s.ignoreSslErrors, intervalMinutes: s.intervalMinutes };
  }

  startAdd() {
    this.editingId = -1;
    this.form = this.blank();
  }

  cancel() { this.editingId = null; }

  save() {
    if (this.editingId === -1) {
      this.uptimeSvc.createService(this.form).subscribe({
        next: () => { this.snack.open('Service added', '', { duration: 2000 }); this.editingId = null; this.load(); },
        error: () => this.snack.open('Error saving', '', { duration: 2000 })
      });
    } else if (this.editingId !== null) {
      this.uptimeSvc.updateService(this.editingId, this.form).subscribe({
        next: () => { this.snack.open('Service updated', '', { duration: 2000 }); this.editingId = null; this.load(); },
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

  private blank(): CreateServiceRequest {
    return { name: '', url: '', isActive: true, ignoreSslErrors: false, intervalMinutes: 5 };
  }
}
