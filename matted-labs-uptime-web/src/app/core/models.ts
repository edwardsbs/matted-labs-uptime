export interface MonitoredService {
  id: number;
  name: string;
  url: string;
  isActive: boolean;
  ignoreSslErrors: boolean;
  intervalMinutes: number;
  createdAt: string;
}

export interface UptimeCheck {
  id: number;
  monitoredServiceId: number;
  checkedAt: string;
  isUp: boolean;
  responseTimeMs: number;
  statusCode: number | null;
  errorMessage: string | null;
}

export interface ServiceStatus {
  service: MonitoredService;
  latestCheck: UptimeCheck | null;
  uptime24h: number;
  uptime7d: number;
  uptime30d: number;
  recentChecks: UptimeCheck[];
}

export interface TestUrlResult {
  isUp: boolean;
  responseTimeMs: number;
  statusCode: number | null;
  errorMessage: string | null;
}

export interface CreateServiceRequest {
  name: string;
  url: string;
  isActive: boolean;
  ignoreSslErrors: boolean;
  intervalMinutes: number;
}
