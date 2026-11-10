import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { I18n } from '@zad/i18n';
import { AdhkarToast } from './features/adhkar/adhkar';
import { TenantList } from './features/tenancy/tenants';

function detectPlatformHost(): boolean {
  if (typeof window === 'undefined') {
    return false;
  }

  return window.location.hostname.toLowerCase().startsWith('admin.');
}

@Component({
  selector: 'app-root',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, AdhkarToast, TenantList],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  readonly i18n = inject(I18n);
  readonly isPlatformHost = detectPlatformHost();
}
