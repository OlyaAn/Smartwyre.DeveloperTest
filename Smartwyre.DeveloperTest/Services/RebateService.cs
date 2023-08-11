using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services;

public class RebateService : IRebateService
{
    private readonly IRebateDataStore _rebateDataStore;
    private readonly IProductDataStore _productDataStore;

    public RebateService(IRebateDataStore rebateDataStore, IProductDataStore productDataStore) {
        _rebateDataStore = rebateDataStore;
        _productDataStore = productDataStore;
    }
    public CalculateRebateResult Calculate(CalculateRebateRequest request)
    {
        Rebate rebate = _rebateDataStore.GetRebate(request.RebateIdentifier);
        Product product = _productDataStore.GetProduct(request.ProductIdentifier);

        var result = new CalculateRebateResult();

        var rebateAmount = SetRebateIncentive(rebate, product, ref result, request);

        if (result.Success)
        {
            var storeRebateDataStore = new RebateDataStore();
            storeRebateDataStore.StoreCalculationResult(rebate, rebateAmount);
        }

        return result;
    }

    private bool IsProductSupportedIncentives(Product product, SupportedIncentiveType supportedIncentivesType) =>
        !product.SupportedIncentives.HasFlag(supportedIncentivesType);

    public decimal SetRebateIncentive(Rebate rebate, Product product, ref CalculateRebateResult result, CalculateRebateRequest request) {

        var rebateAmount = 0m;
        if (rebate == null)
        {
            result.Success = false;
            return rebateAmount;
        }

        switch (rebate.Incentive)
        {
            case IncentiveType.FixedCashAmount:
                rebateAmount = SetRebateIncentiveFixedCashAmount(rebate, product, ref result);
                break;

            case IncentiveType.FixedRateRebate:
                rebateAmount = SetRebateIncentiveFixedRateRebate(rebate, product, ref result, request);
                break;

            case IncentiveType.AmountPerUom:

                rebateAmount = SetRebateIncentiveAmountPerUom(rebate, product, ref result, request);
                break;
        }
        return rebateAmount;

    }
    private decimal SetRebateIncentiveFixedCashAmount(Rebate rebate, Product product, ref CalculateRebateResult result) {
        var rebateAmount = 0m;
        if (!IsProductSupportedIncentives(product, SupportedIncentiveType.FixedCashAmount))
        {
            result.Success = false;
        }
        else if (rebate.Amount == 0)
        {
            result.Success = false;
        }
        else
        {
            result.Success = true;
            rebateAmount = rebate.Amount;
        }
        return rebateAmount;
    }

    private decimal SetRebateIncentiveFixedRateRebate(Rebate rebate, 
        Product product,
        ref CalculateRebateResult result,
        CalculateRebateRequest request)
    {
        var rebateAmount = 0m;
        if (product == null)
        {
            result.Success = false;
        }
        else if (!IsProductSupportedIncentives(product, SupportedIncentiveType.FixedRateRebate))
        {
            result.Success = false;
        }
        else if (rebate.Percentage == 0 || product.Price == 0 || request.Volume == 0)
        {
            result.Success = false;
        }
        else
        {
            rebateAmount += product.Price * rebate.Percentage * request.Volume;
            result.Success = true;
        }
        return rebateAmount;
    }

    private decimal SetRebateIncentiveAmountPerUom(Rebate rebate,
       Product product,
       ref CalculateRebateResult result,
       CalculateRebateRequest request)
    {
        var rebateAmount = 0m;
        if (product == null)
        {
            result.Success = false;
        } else if (!IsProductSupportedIncentives(product, SupportedIncentiveType.AmountPerUom))
        {
            result.Success = false;
        }
        else if (rebate.Amount == 0 || request.Volume == 0)
        {
            result.Success = false;
        }
        else
        {
            rebateAmount += rebate.Amount * request.Volume;
            result.Success = true;
        }
        return rebateAmount;
    }
}
