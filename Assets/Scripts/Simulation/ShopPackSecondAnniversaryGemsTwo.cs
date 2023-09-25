using System;
using Static;

namespace Simulation
{
	public class ShopPackSecondAnniversaryGemsTwo : ShopPack, IHasPackState
	{
		public ShopPackSecondAnniversaryGemsTwo()
		{
			this.tags = ShopPack.Tags.OUT_OF_SHOP;
			this.isOffer = true;
			this.name = "SECOND_ANNIVERSARY_GEMS_PACK";
			this.isIAP = true;
			this.iapIndex = IapIds.SECOND_ANNIVERSARY_GEMS_TWO;
			this.credits = 6000.0;
			this.id = new OfferId?(OfferId.SECOND_ANNIVERSARY_GEMS_TWO);
		}

		public bool isPurchased
		{
			get
			{
				return ShopPackSecondAnniversaryGemsTwo.purchased;
			}
			set
			{
				ShopPackSecondAnniversaryGemsTwo.purchased = value;
			}
		}

		public bool isAppeared
		{
			get
			{
				return ShopPackSecondAnniversaryGemsTwo.appeared;
			}
			set
			{
				ShopPackSecondAnniversaryGemsTwo.appeared = value;
			}
		}

		public override bool CanAppear(Simulator sim)
		{
			if (ShopPackSecondAnniversaryGemsTwo.purchased || !TrustedTime.IsReady())
			{
				return false;
			}
			EventConfig.InternalEvent internalEventConfig = PlayfabManager.eventsInfo.GetInternalEventConfig("secondAnniversary", "gemsOffer2");
			if (internalEventConfig == null)
			{
				return false;
			}
			if (this.totalTime == 0.0)
			{
				this.timeStart = internalEventConfig.startDate;
				this.totalTime = internalEventConfig.durationHours * 3600.0;
			}
			DateTime t = TrustedTime.Get();
			return t >= this.timeStart && t < internalEventConfig.endDate;
		}

		public override void OnAppeared()
		{
			base.OnAppeared();
			PlayerStats.OnOfferOffered(OfferId.SECOND_ANNIVERSARY_GEMS_TWO);
			ShopPackSecondAnniversaryGemsTwo.appeared = true;
		}

		public override void OnCheckout()
		{
			base.OnCheckout();
			PlayerStats.OnOfferCheckout(OfferId.SECOND_ANNIVERSARY_GEMS_TWO);
		}

		public override void OnPurchaseCompleted()
		{
			base.OnPurchaseCompleted();
			PlayerStats.OnOfferAccepted(OfferId.SECOND_ANNIVERSARY_GEMS_TWO);
			ShopPackSecondAnniversaryGemsTwo.purchased = true;
		}

		public override bool ShouldOverrideExisting(Simulator sim)
		{
			return true;
		}

		public override float GetChanceWeight(Simulator sim)
		{
			return 300f;
		}

		public override string GetName()
		{
			return string.Format(LM.Get("SECOND_ANNIVERSARY_GEMS_PACK"), 1);
		}

		public static bool appeared;

		public static bool purchased;
	}
}
