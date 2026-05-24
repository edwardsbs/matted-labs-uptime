import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { interval, Subscription, startWith, switchMap } from 'rxjs';
import { UptimeService } from '../../core/uptime.service';
import { ServiceStatus, UptimeCheck } from '../../core/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatCardModule, MatChipsModule, MatIconModule,
    MatButtonModule, MatProgressSpinnerModule, MatTooltipModule
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
  private uptimeSvc = inject(UptimeService);

  statuses = signal<ServiceStatus[]>([]);
  loading = signal(true);

  downCount = computed(() => this.statuses().filter(s => s.latestCheck && !s.latestCheck.isUp).length);
  allUp = computed(() => this.downCount() === 0 && this.statuses().length > 0);

  private sub?: Subscription;

  ngOnInit() {
    this.sub = interval(30000).pipe(
      startWith(0),
      switchMap(() => this.uptimeSvc.getDashboard())
    ).subscribe({
      next: data => { this.statuses.set(data); this.loading.set(false); },
      error: () => { this.loading.set(false); }
    });
  }

  ngOnDestroy() { this.sub?.unsubscribe(); }

  sparkColor(c: UptimeCheck) { return c.isUp ? '#4caf50' : '#f44336'; }

  formatTime(iso: string) {
    return new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  formatAgo(iso: string) {
    const m = Math.floor((Date.now() - new Date(iso).getTime()) / 60000);
    if (m < 1) return 'just now';
    if (m < 60) return `${m}m ago`;
    return `${Math.floor(m / 60)}h ago`;
  }
}
