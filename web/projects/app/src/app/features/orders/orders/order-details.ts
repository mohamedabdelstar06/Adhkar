import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { I18n } from '@zad/i18n';
import { OrderDetailsDto } from './order.model';
import { OrderState } from './order-state';

@Component({
  selector: 'zad-order-details',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, DecimalPipe],
  templateUrl: './order-details.html',
  styleUrl: './order-details.scss'
})
export class OrderDetails {
  readonly i18n = inject(I18n);
  protected readonly state = inject(OrderState);

  readonly order = input.required<OrderDetailsDto>();
}

