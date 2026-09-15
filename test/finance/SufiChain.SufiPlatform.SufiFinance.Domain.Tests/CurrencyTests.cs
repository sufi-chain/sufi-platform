using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Currencies;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Tests;public class CurrencyTests{[Fact]public void Should_Model_Irt_As_Ten_Irr(){var c=new Currency(Guid.NewGuid(),null,"irt","Iranian Toman","تومان",0,baseUnitCode:"IRR",baseUnitRatio:10);c.Code.ShouldBe("IRT");c.BaseUnitCode.ShouldBe("IRR");c.BaseUnitRatio.ShouldBe(10);} [Fact]public void Should_Reject_Self_Unit(){Should.Throw<BusinessException>(()=>new Currency(Guid.NewGuid(),null,"USD","Dollar","$",2,baseUnitCode:"USD"));}}
