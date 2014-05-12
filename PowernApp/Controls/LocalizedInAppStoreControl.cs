using PhoneKit.Framework.Controls;
using PowernApp.Resources;
using System.Windows.Media;

namespace PowernApp.Controls
{
    public class LocalizedInAppStoreControl : InAppStoreControlBase
    {
        public LocalizedInAppStoreControl()
        {
            BackgroundTheme.Color = (Color)App.Current.Resources["ThemeBackgroundMediumColor"];
        }

        /// <summary>
        /// Localizes the user control content and texts.
        /// </summary>
        protected override void LocalizeContent()
        {
            InAppStoreLoadingText = AppResources.InAppStoreLoading;
            InAppStoreNoProductsText = AppResources.InAppStoreNoProducts;
            InAppStorePurchasedText = AppResources.InAppStorePurchased;
            SupportedProductIds = AppConstants.PRO_VERSION_IN_APP_KEY;
        }
    }
}
