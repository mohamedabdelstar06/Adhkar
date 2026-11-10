using FluentValidation;

namespace Zad.Modules.Orders.Application.Orders;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(request => request.CustomerEmail)
            .NotEmpty().WithMessage("Customer email is required")
            .EmailAddress().WithMessage("A valid email address is required");

        RuleFor(request => request.Items)
            .NotEmpty().WithMessage("Order must contain at least one item");

        RuleForEach(request => request.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("Product ID is required");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero");
        });
    }
}

