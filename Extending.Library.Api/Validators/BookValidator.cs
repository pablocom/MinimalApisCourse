﻿using Extending.Library.Api.Models;
using FluentValidation;

namespace Extending.Library.Api.Validators;

public class BookValidator : AbstractValidator<Book>
{
    public BookValidator()
    {
        RuleFor(book => book.Isbn)
            .Matches(@"[0-9]*[-| ][0-9]*[-| ][0-9]*[-| ][0-9]*[-| ][0-9]*")
            .WithMessage("Value was not a valid ISBN-13");

        RuleFor(book => book.Title).NotEmpty();
        RuleFor(book => book.ShortDescription).NotEmpty();
        RuleFor(book => book.PageCount).GreaterThan(0);
        RuleFor(book => book.Author).NotEmpty();
    }
}
