using PhoneKit.Framework.Controls;
using PowernApp.Resources;
using System;
using System.Collections.Generic;

namespace PowernApp.Controls
{
    /// <summary>
    /// The localized About control.
    /// </summary>
    public class LocalizedAboutControl : ThemedAboutControlBase
    {
        /// <summary>
        /// Localizes the user controls contents and texts.
        /// </summary>
        protected override void LocalizeContent()
        {
            ApplicationIconSource = new Uri("/Assets/ApplicationIcon.png", UriKind.Relative);
            ApplicationTitle = AppResources.ApplicationTitle;
            ApplicationVersion = AppResources.ApplicationVersion;
            ApplicationAuthor= AppResources.ApplicationAuthor;
            ApplicationDescription = AppResources.ApplicationDescription;
            SupportAndFeedbackText = AppResources.SupportAndFeedback;
            SupportAndFeedbackEmail = "apps@bsautermeister.de";
            PrivacyInfoText= AppResources.PrivacyInfo;
            PrivacyInfoLink= "http://bsautermeister.de/privacy.php";
            RateAndReviewText = AppResources.RateAndReview;
            MoreAppsText= AppResources.MoreApps;
            MoreAppsSearchTerms = "Benjamin Sautermeister";

            // contributors
            ContributorsListVisibility = System.Windows.Visibility.Visible;
            SetContributorsList(new List<ContributorModel>() {
                new ContributorModel("/Assets/Languages/french.png","Maël Navarro Salcedo"),
                new ContributorModel("/Assets/Languages/portuguese_br.png","João Vitório Dagostin"),
                new ContributorModel("/Assets/Languages/indonesian.png","Agus Setiawan"),
                new ContributorModel("/Assets/Languages/italiano.png","Roc Lat"),
                new ContributorModel("/Assets/Languages/spanish.png", "Juan Febrero"),
                new ContributorModel("/Assets/Languages/russia.png", "Иван Скороходов"),
                new ContributorModel("/Assets/Languages/persian.png", "Mahmud Karimi"),
                new ContributorModel("/Assets/Languages/chinese.png", "杨博涵")
            });
        }
    }
}
