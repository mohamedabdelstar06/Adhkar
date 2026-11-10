import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { API_ROUTES } from '../../../api/api-base';
import { AdhkarDto, AdhkarUpsertRequest } from './adhkar.model';

@Injectable()
export class AdhkarState {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = API_ROUTES.adhkar;

  readonly items = signal<AdhkarDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly createOpen = signal(false);
  readonly editingDhikr = signal<AdhkarDto | null>(null);
  readonly error = signal<string | null>(null);

  reload(): void {
    this.loading.set(true);
    this.http.get<AdhkarDto[]>(`${this.baseUrl}?includeInactive=true`).subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load adhkar');
        this.loading.set(false);
      }
    });
  }

  openCreate(): void {
    this.error.set(null);
    this.editingDhikr.set(null);
    this.createOpen.set(true);
  }

  closeCreate(): void {
    this.createOpen.set(false);
    this.error.set(null);
  }

  openUpdate(dhikr: AdhkarDto): void {
    this.error.set(null);
    this.createOpen.set(false);
    this.editingDhikr.set(dhikr);
  }

  closeUpdate(): void {
    this.editingDhikr.set(null);
    this.error.set(null);
  }

  create(request: AdhkarUpsertRequest): void {
    this.saving.set(true);
    this.error.set(null);
    this.http.post<AdhkarDto>(this.baseUrl, request).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeCreate();
        this.reload();
      },
      error: (response: { error?: { title?: string } }) => {
        this.saving.set(false);
        this.error.set(response?.error?.title ?? 'Could not save the dhikr');
      }
    });
  }

  update(id: string, request: AdhkarUpsertRequest): void {
    this.saving.set(true);
    this.error.set(null);
    this.http.put<void>(`${this.baseUrl}/${id}`, request).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeUpdate();
        this.reload();
      },
      error: (response: { error?: { title?: string } }) => {
        this.saving.set(false);
        this.error.set(response?.error?.title ?? 'Could not save the dhikr');
      }
    });
  }

  remove(id: string): void {
    this.http.delete<void>(`${this.baseUrl}/${id}`).subscribe({
      next: () => this.reload(),
      error: () => this.error.set('Could not delete the dhikr')
    });
  }
}

