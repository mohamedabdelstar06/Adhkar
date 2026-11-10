import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { I18n } from '@zad/i18n';
import { ZadButton, ZadPageHeader } from '@zad/ui';
import { AdhkarCreate } from './adhkar-create';
import { AdhkarDto } from './adhkar.model';
import { AdhkarState } from './adhkar-state';
import { AdhkarUpdate } from './adhkar-update';

@Component({
  selector: 'zad-adhkar-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ZadPageHeader, ZadButton, AdhkarCreate, AdhkarUpdate],
  templateUrl: './adhkar-list.html',
  styleUrl: './adhkar-list.scss'
})
export class AdhkarList implements OnInit {
  readonly i18n = inject(I18n);
  protected readonly state = inject(AdhkarState);

  ngOnInit(): void {
    this.state.reload();
  }

  remove(dhikr: AdhkarDto): void {
    if (confirm(this.i18n.t('adhkar.deleteConfirm'))) {
      this.state.remove(dhikr.id);
    }
  }
}

