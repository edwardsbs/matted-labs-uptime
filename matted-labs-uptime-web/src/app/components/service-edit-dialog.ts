import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { UptimeService } from '../core/uptime.service';
import { MonitoredService, CreateServiceRequest, TestUrlResult } from '../core/models';

@Component({
  selector: 'app-service-edit-dialog',
  standalone: true,
  imports: [
    CommonModule, MatDialogModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatSlideToggleModule,
    MatProgressSpinnerModule, MatSnackBarModule
  ],
  templateUrl: './service-edit-dialog.html',
  styleUrl: './service-edit-dialog.scss'
})
export class ServiceEditDialogComponent {
  private dialogRef = inject(MatDialogRef<ServiceEditDialogComponent>);
  private uptimeSvc = inject(UptimeService);
  private snack = inject(MatSnackBar);
  readonly service = inject<MonitoredService>(MAT_DIALOG_DATA);

  form = signal<CreateServiceRequest>({
    name: this.service.name,
    url: this.service.url,
    isActive: this.service.isActive,
    ignoreSslErrors: this.service.ignoreSslErrors,
    intervalMinutes: this.service.intervalMinutes
  });
  testResult = signal<TestUrlResult | null>(null);
  testing = signal(false);
  saving = signal(false);

  updateForm(patch: Partial<CreateServiceRequest>) {
    this.form.update(f => ({ ...f, ...patch }));
    if (patch.url !== undefined || patch.ignoreSslErrors !== undefined)
      this.testResult.set(null);
  }

  testUrl() {
    const { url, ignoreSslErrors } = this.form();
    if (!url) return;
    this.testing.set(true);
    this.testResult.set(null);
    this.uptimeSvc.testUrl(url, ignoreSslErrors).subscribe({
      next: r => { this.testResult.set(r); this.testing.set(false); },
      error: () => this.testing.set(false)
    });
  }

  save() {
    this.saving.set(true);
    this.uptimeSvc.updateService(this.service.id, this.form()).subscribe({
      next: () => this.dialogRef.close(true),
      error: () => { this.saving.set(false); this.snack.open('Error saving', '', { duration: 2000 }); }
    });
  }

  cancel() { this.dialogRef.close(false); }
}
