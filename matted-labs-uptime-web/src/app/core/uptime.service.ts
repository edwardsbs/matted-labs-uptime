import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { MonitoredService, ServiceStatus, UptimeCheck, CreateServiceRequest, TestUrlResult, AppSettings } from './models';

@Injectable({ providedIn: 'root' })
export class UptimeService {
  private http = inject(HttpClient);
  private base = environment.apiBase;

  getDashboard(): Observable<ServiceStatus[]> {
    return this.http.get<ServiceStatus[]>(`${this.base}/checks/dashboard`);
  }

  getServices(): Observable<MonitoredService[]> {
    return this.http.get<MonitoredService[]>(`${this.base}/services`);
  }

  getService(id: number): Observable<MonitoredService> {
    return this.http.get<MonitoredService>(`${this.base}/services/${id}`);
  }

  createService(req: CreateServiceRequest): Observable<MonitoredService> {
    return this.http.post<MonitoredService>(`${this.base}/services`, req);
  }

  updateService(id: number, req: CreateServiceRequest): Observable<MonitoredService> {
    return this.http.put<MonitoredService>(`${this.base}/services/${id}`, req);
  }

  deleteService(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/services/${id}`);
  }

  getHistory(serviceId: number, limit = 200): Observable<UptimeCheck[]> {
    return this.http.get<UptimeCheck[]>(`${this.base}/checks/${serviceId}?limit=${limit}`);
  }

  checkNow(serviceId: number): Observable<UptimeCheck> {
    return this.http.post<UptimeCheck>(`${this.base}/checks/${serviceId}/check-now`, {});
  }

  testUrl(url: string, ignoreSslErrors: boolean): Observable<TestUrlResult> {
    return this.http.post<TestUrlResult>(`${this.base}/checks/test`, { url, ignoreSslErrors });
  }

  getSettings(): Observable<AppSettings> {
    return this.http.get<AppSettings>(`${this.base}/settings`);
  }

  updateSettings(settings: AppSettings): Observable<AppSettings> {
    return this.http.put<AppSettings>(`${this.base}/settings`, settings);
  }

  sendTestEmail(): Observable<{ sent: boolean }> {
    return this.http.post<{ sent: boolean }>(`${this.base}/settings/test-email`, {});
  }
}
