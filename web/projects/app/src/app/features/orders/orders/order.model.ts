export interface OrderItemDto {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface OrderListItemDto {
  id: string;
  orderNumber: string;
  customerEmail: string;
  totalAmount: number;
  status: string;
  itemCount: number;
  createdAtUtc: string;
}

export interface OrderDetailsDto {
  id: string;
  orderNumber: string;
  customerEmail: string;
  totalAmount: number;
  status: string;
  items: OrderItemDto[];
  createdAtUtc: string;
}

export interface CreateOrderItemRequest {
  productId: string;
  quantity: number;
}

export interface CreateOrderRequest {
  customerEmail: string;
  items: CreateOrderItemRequest[];
}

export interface OrderProductOption {
  id: string;
  sku: string;
  name: string;
  unitPrice: number;
  stockQuantity: number;
  isActive: boolean;
}

export interface OrderPagedList<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface CartLineItem {
  product: OrderProductOption;
  quantity: number;
}
