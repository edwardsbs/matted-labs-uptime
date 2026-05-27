import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { UptimeService } from '../../core/uptime.service';
import { AppSettings } from '../../core/models';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatSlideToggleModule,
    MatProgressSpinnerModule, MatSnackBarModule, MatTooltipModule,
    MatDividerModule
  ],
  templateUrl: './settings.html',
  styleUrl: './settings.scss'
})
export class SettingsComponent implements OnInit {
  private uptimeSvc = inject(UptimeService);
  private snack = inject(MatSnackBar);

  loading = signal(true);
  saving = signal(false);
  sendingTest = signal(false);
  showPassword = signal(false);

  form = signal<AppSettings>({
    smtpHost: '',
    smtpPort: 587,
    smtpUser: '',
    smtpPassword: '',
    smtpFrom: '',
    alertRecipient: '',
    smtpEnableSsl: true,
    alertsEnabled: true
  });

  ngOnInit() {
    this.uptimeSvc.getSettings().subscribe({
      next: s => { this.form.set(s); this.loading.set(false); },
      error: () => { this.snack.open('Failed to load settings', '', { duration: 3000 }); this.loading.set(false); }
    });
  }

  update(patch: Partial<AppSettings>) {
    this.form.update(f => ({ ...f, ...patch }));
  }

  save() {
    this.saving.set(true);
    this.uptimeSvc.updateSettings(this.form()).subscribe({
      next: () => { this.snack.open('Settings saved', '', { duration: 2000 }); this.saving.set(false); },
      error: () => { this.snack.open('Error saving settings', '', { duration: 2000 }); this.saving.set(false); }
    });
  }

  sendTestEmail() {
    this.sendingTest.set(true);
    this.uptimeSvc.sendTestEmail(this.form()).subscribe({
      next: r => {
        this.snack.open(
          r.sent ? 'Test email sent — check your inbox' : `Not sent: ${r.error ?? 'Unknown error'}`,
          '', { duration: 8000 }
        );
        this.sendingTest.set(false);
      },
      error: () => { this.snack.open('Error sending test email', '', { duration: 2000 }); this.sendingTest.set(false); }
    });
  }
}
