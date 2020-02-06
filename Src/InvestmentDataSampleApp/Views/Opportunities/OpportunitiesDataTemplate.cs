using System.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using Xamarin.Forms;

namespace InvestmentDataSampleApp
{
    class OpportunitiesDataTemplateSelector : DataTemplateSelector
    {
        readonly static int _rowHeight = Device.Idiom is TargetIdiom.Phone ? 50 : 150;

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
            new OpportunitiesDataTemplate((OpportunityModel)item);

        class OpportunitiesDataTemplate : DataTemplate
        {

            public OpportunitiesDataTemplate(OpportunityModel opportunityModel) : base(() => LoadTemplate(opportunityModel))
            {

            }

            static SwipeView LoadTemplate(OpportunityModel opportunityModel)
            {
                var beaconFundingImage = new Image
                {
                    Source = "beaconfundingicon",
                    HeightRequest = _rowHeight
                };

                var topicTitleLabel = new Label
                {
                    Text = "Topic",
                    FontAttributes = FontAttributes.Bold,
                    VerticalTextAlignment = TextAlignment.End
                };

                var topicDescriptionLabel = new Label
                {
                    VerticalTextAlignment = TextAlignment.Start,
                    Text = opportunityModel.Topic
                };

                var companyTitleLabel = new Label
                {
                    Text = "Company",
                    FontAttributes = FontAttributes.Bold,
                    VerticalTextAlignment = TextAlignment.End
                };

                var companyDescriptionLabel = new Label
                {
                    VerticalTextAlignment = TextAlignment.Start,
                    Text = opportunityModel.Company
                };

                var leaseAmountTitleLabel = new Label
                {
                    Text = "Lease Amount",
                    FontAttributes = FontAttributes.Bold,
                    VerticalTextAlignment = TextAlignment.End
                };

                var leaseAmountDescriptionLabel = new Label
                {
                    VerticalTextAlignment = TextAlignment.Start,
                    Text = opportunityModel.LeaseAmountAsCurrency
                };

                var ownerTitleLabel = new Label
                {
                    Text = "Owner",
                    FontAttributes = FontAttributes.Bold,
                    VerticalTextAlignment = TextAlignment.End
                };

                var ownerDescriptionLabel = new Label
                {
                    VerticalTextAlignment = TextAlignment.Start,
                    Text = opportunityModel.Owner
                };

                var grid = new Grid
                {
                    BackgroundColor = Color.White,

                    Padding = new Thickness(5, 0, 0, 0),

                    ColumnSpacing = 20,
                    RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                },
                    ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(_rowHeight / 3, GridUnitType.Absolute) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
                };

                grid.Children.Add(beaconFundingImage, 0, 0);
                Grid.SetRowSpan(beaconFundingImage, 2);

                grid.Children.Add(topicTitleLabel, 1, 0);
                grid.Children.Add(topicDescriptionLabel, 1, 1);

                if (Device.Idiom != TargetIdiom.Phone)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


                    grid.Children.Add(companyTitleLabel, 2, 0);
                    grid.Children.Add(companyDescriptionLabel, 2, 1);

                    grid.Children.Add(leaseAmountTitleLabel, 3, 0);
                    grid.Children.Add(leaseAmountDescriptionLabel, 3, 1);

                    grid.Children.Add(ownerTitleLabel, 4, 0);
                    grid.Children.Add(ownerDescriptionLabel, 4, 1);
                }

                var deleteSwipeItem = new SwipeItem
                {
                    Text = "Delete",
                    IconImageSource = "Delete",
                    BackgroundColor = Color.Red,
                    Command = new AsyncCommand<string>(ExecuteSwipeToDeleteCommand)
                };
                deleteSwipeItem.SetBinding(SwipeItem.CommandParameterProperty, nameof(OpportunityModel.Topic));

                var rightSwipeItems = new SwipeItems
                {
                    Mode = SwipeMode.Execute
                };
                rightSwipeItems.Add(deleteSwipeItem);

                var swipeViewTappedCommand = new AsyncCommand<OpportunityModel>(ExecuteSwipeViewTappedCommand);

                var swipeView = new ExtendedSwipeView(swipeViewTappedCommand, opportunityModel)
                {
                    RightItems = rightSwipeItems,
                    Content = grid,
                    Margin = new Thickness(0, 5, 0, 15)
                };

                return swipeView;
            }

            static Task ExecuteSwipeViewTappedCommand(OpportunityModel opportunityModel) =>
                Application.Current.MainPage.Navigation.PushAsync(new OpportunityDetailsPage(opportunityModel));

            static async Task ExecuteSwipeToDeleteCommand(string topic)
            {
                var opportunityModelToDelete = await OpportunityModelDatabase.GetOpportunityByTopic(topic).ConfigureAwait(false);
                await OpportunityModelDatabase.DeleteItem(opportunityModelToDelete).ConfigureAwait(false);

                await TriggerPullToRefresh().ConfigureAwait(false);
            }

            static Task TriggerPullToRefresh()
            {
                var navigationPage = (NavigationPage)Application.Current.MainPage;
                var opportunityPage = navigationPage.Navigation.NavigationStack.OfType<OpportunitiesPage>().First();

                var opportunityPageLayout = (Layout<View>)opportunityPage.Content;
                var refreshView = opportunityPageLayout.Children.OfType<RefreshView>().First();

                return Device.InvokeOnMainThreadAsync(() => refreshView.IsRefreshing = true);
            }
        }
    }
}

