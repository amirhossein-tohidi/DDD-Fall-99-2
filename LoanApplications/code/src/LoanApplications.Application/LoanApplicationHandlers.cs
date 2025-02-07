﻿using System;
using System.Threading.Tasks;
using Framework.Application;
using LoanApplications.Application.Contracts.LoanApplications;
using LoanApplications.Domain.Model.LoanApplications;
using LoanApplications.Domain.Services;

namespace LoanApplications.Application
{
    public class LoanApplicationHandlers : ICommandHandler<PlaceLoanApplication>,
                                            ICommandHandler<RejectLoanApplication>,
                                            ICommandHandler<ConfirmLoanApplicationWithConsiderations>,
                                            ICommandHandler<ConfirmLoanApplication>,
                                            ICommandHandler<CancelLoanApplication>
    {
        private readonly ILoanApplicationRepository _repository;
        private readonly IInterestRateCalculator _interestRateCalculator;
        public LoanApplicationHandlers(ILoanApplicationRepository repository, IInterestRateCalculator interestRateCalculator)
        {
            _repository = repository;
            _interestRateCalculator = interestRateCalculator;
        }

        public async Task Handle(PlaceLoanApplication command)
        {
            var paybackPeriod = TimeSpan.FromDays(command.PaybackPeriodDays);
            var interestRate = await _interestRateCalculator.CalculateInterestRateForApplicant(command.ApplicantId);
            var id = await _repository.NextId();
            var loanApplication = new LoanApplication(id, command.ApplicantId, command.LoanAmount, paybackPeriod, interestRate);
            _repository.Add(loanApplication);
        }

        public async Task Handle(RejectLoanApplication command)
        {
            var loanApplication = await _repository.Get(command.ApplicationId);
            loanApplication.Reject();
        }

        public async Task Handle(ConfirmLoanApplication command)
        {
            var loanApplication = await _repository.Get(command.ApplicationId);
            loanApplication.Confirm();
        }
        public async Task Handle(ConfirmLoanApplicationWithConsiderations command)
        {
            var loanApplication = await _repository.Get(command.ApplicationId);
            var paybackPeriod = TimeSpan.FromDays(command.PaybackPeriodDays);
            loanApplication.Confirm(command.LoanAmount, paybackPeriod);
        }

        public async Task Handle(CancelLoanApplication command)
        {
            var loanApplication = await _repository.Get(command.ApplicationId);
            loanApplication.Cancel();
        }
    }
}
