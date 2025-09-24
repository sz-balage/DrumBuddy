namespace DrumBuddy;

// public class AppViewLocator : IViewLocator
// {
//     public IViewFor ResolveView<T>(T viewModel, string contract = null)
//     {
//         return viewModel switch
//         {
//             HomeViewModel context => new Views.HomeView { ViewModel = context },
//             RecordingViewModel context => new RecordingView { ViewModel = context },
//             ILibraryViewModel context => new LibraryView { ViewModel = context },
//             _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
//         };
//     }
// }