using PowernApp.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowernApp.Controls
{
    public class LocalizedInAppStoreControl : ThemedInAppStoreControlBase
    {
        /// <summary>
        /// Localizes the user control content and texts.
        /// </summary>
        protected override void LocalizeContent()
        {
            InAppStoreLoadingText = AppResources.InAppStoreLoading;
            InAppStoreNoProductsText = AppResources.InAppStoreNoProducts;
            InAppStorePurchasedText = AppResources.InAppStorePurchased;
            SupportedProductIds = "pro_version";
        }
    }
}
