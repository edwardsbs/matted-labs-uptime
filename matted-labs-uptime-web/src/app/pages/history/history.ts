import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { switchMap } from 'rxjs';
import { UptimeService } from '../../core/uptime.service';
import { MonitoredService, UptimeCheck } from '../../core/models';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatTableModule, MatCardModule, MatIconModule,
    MatButtonModule, MatProgressSpinnerModule, MatTooltipModule
  ],
  templateUrl: './history.html',
  styleUrl: './history.scss'
})
export class HistoryComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private uptimeSvc = inject(UptimeService);

  service = signal<MonitoredService | undefined>(undefined);
  checks = signal<UptimeCheck[]>([]);
  loading = signal(true);
  displayedColumns = ['status', 'checkedAt', 'responseTime', 'statusCode', 'error'];

  ngOnInit() {
    this.route.params.pipe(
      switchMap(p => {
        const id = +p['id'];
        this.uptimeSvc.getService(id).subscribe(s => this.service.set(s));
        return this.uptimeSvc.getHistory(id, 200);
      })
    ).subscribe({
      next: checks => { this.checks.set(checks); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  checkNow() {
    const svc = this.service();
    if (!svc) return;
    this.uptimeSvc.checkNow(svc.id).subscribe(() => {
      this.uptimeSvc.getHistory(svc.id, 200).subscribe(c => this.checks.set(c));
    });
  }
}
