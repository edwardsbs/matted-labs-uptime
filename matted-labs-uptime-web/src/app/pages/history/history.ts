import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { switchMap } from 'rxjs';
import { UptimeService } from '../../core/uptime.service';
import { MonitoredService, UptimeCheck } from '../../core/models';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatTableModule, MatCardModule, MatIconModule,
    MatButtonModule, MatProgressSpinnerModule, MatChipsModule
  ],
  templateUrl: './history.html',
  styleUrl: './history.scss'
})
export class HistoryComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private uptimeSvc = inject(UptimeService);

  service?: MonitoredService;
  checks: UptimeCheck[] = [];
  loading = true;
  displayedColumns = ['status', 'checkedAt', 'responseTime', 'statusCode', 'error'];

  ngOnInit() {
    this.route.params.pipe(
      switchMap(p => {
        const id = +p['id'];
        this.uptimeSvc.getService(id).subscribe(s => this.service = s);
        return this.uptimeSvc.getHistory(id, 200);
      })
    ).subscribe({
      next: checks => { this.checks = checks; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  checkNow() {
    if (!this.service) return;
    this.uptimeSvc.checkNow(this.service.id).subscribe(() => {
      this.uptimeSvc.getHistory(this.service!.id, 200).subscribe(c => this.checks = c);
    });
  }
}
