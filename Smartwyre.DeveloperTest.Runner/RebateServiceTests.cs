
using Moq;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;
using System;
using Xunit;

namespace Smartwyre.DeveloperTest.Runner
{
    public class RebateServiceTests
    {
        [Theory]
        [InlineData(IncentiveType.FixedCashAmount, 1, SupportedIncentiveType.FixedCashAmount, 0, 0, 0, 0, false)]
        [InlineData(IncentiveType.FixedCashAmount, 0, SupportedIncentiveType.FixedCashAmount, 0, 0, 0, 0, false)]
        [InlineData(IncentiveType.FixedCashAmount, 1, SupportedIncentiveType.FixedRateRebate, 0, 0, 0, 1, true)]
        [InlineData(IncentiveType.FixedCashAmount, 0, SupportedIncentiveType.FixedRateRebate, 0, 0, 0, 0, false)]
        [InlineData(IncentiveType.FixedRateRebate, 0, SupportedIncentiveType.FixedRateRebate, 0, 0, 0, 0, false)]
        [InlineData(IncentiveType.FixedRateRebate, 0, SupportedIncentiveType.FixedRateRebate, 1, 0, 0, 0, false)]
        [InlineData(IncentiveType.FixedRateRebate, 0, SupportedIncentiveType.FixedRateRebate, 1, 1, 0, 0, false)]
        [InlineData(IncentiveType.FixedRateRebate, 0, SupportedIncentiveType.FixedCashAmount, 1, 1, 2, 2, true)]
        [InlineData(IncentiveType.AmountPerUom, 1, SupportedIncentiveType.FixedCashAmount, 1, 1, 2, 2, true)]
        [InlineData(IncentiveType.AmountPerUom, 1, SupportedIncentiveType.AmountPerUom, 1, 1, 2, 0, false)]
        [InlineData(IncentiveType.AmountPerUom, 0, SupportedIncentiveType.FixedCashAmount, 1, 1, 2, 0, false)]
        [InlineData(IncentiveType.AmountPerUom, 1, SupportedIncentiveType.FixedCashAmount, 1, 1, 0, 0, false)]
        public void SetRebateIncentiveWhenProductNotNull(IncentiveType incentiveType,
            decimal amount,
            SupportedIncentiveType supportedIncentivesType,
            decimal percentage,
            decimal price,
            decimal volume,
            decimal rebateAmount,
            bool success
            )
        {
            var rebateDataStore = new Mock<IRebateDataStore>();
            var productDataStore = new Mock<IProductDataStore>();
            var rebateService = new RebateService(rebateDataStore.Object, productDataStore.Object);
            var result = new CalculateRebateResult();

            var request = new CalculateRebateRequest()
            {
                Volume = volume
            };

            var product = new Product()
            {
                Price = price,
                SupportedIncentives = supportedIncentivesType
            };

            var rebate = new Rebate()
            {
                Amount = amount,
                Incentive = incentiveType,
                Percentage = percentage,
            };

            var res = rebateService.SetRebateIncentive(rebate, product, ref result, request);

            Assert.True(result.Success == success);
            Assert.True(res  == rebateAmount);
        }

        [Theory]
        [InlineData(IncentiveType.FixedRateRebate)]
        [InlineData(IncentiveType.AmountPerUom)]
        public void SetRebateIncentiveWhenProductNull(IncentiveType incentiveType)
        {
            var rebateDataStore = new Mock<IRebateDataStore>();
            var productDataStore = new Mock<IProductDataStore>();
            var rebateService = new RebateService(rebateDataStore.Object, productDataStore.Object);
            var result = new CalculateRebateResult();

            var rebate = new Rebate()
            {
                Amount = 0,
                Incentive = incentiveType,
                Percentage = 0,
            };

            var res = rebateService.SetRebateIncentive(rebate, null, ref result, null);

            Assert.True(result.Success == false);
            Assert.True(res == 0);
        }

        [Fact]
        public void SetRebateIncentiveWhenRebateNull()
        {
            var rebateDataStore = new Mock<IRebateDataStore>();
            var productDataStore = new Mock<IProductDataStore>();
            var rebateService = new RebateService(rebateDataStore.Object, productDataStore.Object);
            var result = new CalculateRebateResult();

            var res = rebateService.SetRebateIncentive(null, null, ref result, null);

            Assert.True(result.Success == false);
            Assert.True(res == 0);
        }
    }
}
