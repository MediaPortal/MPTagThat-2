using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPTagThat.Core.Common.Song;
using Prism.Mvvm;
using Prism.Regions;

namespace MPTagThat.TagEdit.ViewModels
{
  public class TagEditViewModel : BindableBase, INavigationAware
  {
    #region Variables

    #endregion

    #region Properties

    private SongData _songEdit;
    public SongData SongEdit
    {
      get => _songEdit;
      set => SetProperty(ref _songEdit, value);
    }

    #endregion

    #region ctor

    public TagEditViewModel()
    {

    }

    #endregion

    #region Interface

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
      var songs = navigationContext.Parameters["songs"] as List<SongData>;
      SongEdit = songs[0];
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
      return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
      // Clear the view
    }

    #endregion
  }
}
