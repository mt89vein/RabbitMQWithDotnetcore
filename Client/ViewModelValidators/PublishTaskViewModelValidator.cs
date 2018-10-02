using Client.ViewModels;
using FluentValidation;
using MQ.Messages;

namespace Client.ViewModelValidators
{
    public abstract class PublishTaskViewModelValidator<TUserInputData> : AbstractValidator<PublishTaskViewModel<TUserInputData>>
        where TUserInputData: UserInputData
    {
        protected PublishTaskViewModelValidator()
        {
            RuleFor(w => w.UserInputData.Login)
                .NotNull().WithMessage("Не задан логин");

            RuleFor(w => w.UserInputData.Password)
                .NotNull().WithMessage("Не задан пароль");
        }
    }

    public class DocumentOnePublishTaskViewModelValidator : PublishTaskViewModelValidator<DocumentOnePublishUserInputData>
    {
        public DocumentOnePublishTaskViewModelValidator()
        {
            RuleFor(w => w.UserInputData.RegistryNumber)
                .NotNull().WithMessage("Реестровый номер не задан");

            RuleFor(w => w.UserInputData.Password)
                .NotNull().WithMessage("Не задан пароль");
        }
    }
}