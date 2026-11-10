import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { I18n } from '@zad/i18n';
import { API_ROUTES } from '../../../api/api-base';
import { AdhkarDto } from './adhkar.model';

const POLL_INTERVAL_MS = 60_000;
const VISIBLE_FOR_MS = 9_000;

@Component({
  selector: 'zad-adhkar-toast',
  templateUrl: './adhkar-toast.html',
  styleUrl: './adhkar-toast.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdhkarToast implements OnInit, OnDestroy {
  private readonly http = inject(HttpClient);
  readonly i18n = inject(I18n);

  readonly current = signal<AdhkarDto | null>(null);
  readonly visible = signal(false);

  private pollTimer: ReturnType<typeof setInterval> | undefined;
  private hideTimer: ReturnType<typeof setTimeout> | undefined;

  ngOnInit(): void {
    this.refresh();
    this.pollTimer = setInterval(() => this.refresh(), POLL_INTERVAL_MS);
  }

  ngOnDestroy(): void {
    if (this.pollTimer) {
      clearInterval(this.pollTimer);
    }

    if (this.hideTimer) {
      clearTimeout(this.hideTimer);
    }
  }

  dismiss(): void {
    this.visible.set(false);
  }

  private refresh(): void {
    this.http.get<AdhkarDto>(`${API_ROUTES.adhkar}/current`).subscribe({
      next: (dhikr) => {
        this.current.set(dhikr);
        this.visible.set(true);
        this.scheduleHide();
      },
      error: () => this.visible.set(false)
    });
  }

  private scheduleHide(): void {
    if (this.hideTimer) {
      clearTimeout(this.hideTimer);
    }

    this.hideTimer = setTimeout(() => this.visible.set(false), VISIBLE_FOR_MS);
  }
}

