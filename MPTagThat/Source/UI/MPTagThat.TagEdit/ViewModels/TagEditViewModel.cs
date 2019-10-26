using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MPTagThat.Core;
using MPTagThat.Core.Common;
using MPTagThat.Core.Common.Song;
using MPTagThat.Core.Events;
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
      _applyEditCommand = new BaseCommand(ApplyEdit);
    }

    #endregion

    #region Commands

    private ICommand _applyEditCommand;
    public ICommand ApplyEditCommand => _applyEditCommand;

    private void ApplyEdit(object param)
    {
      var evt = new GenericEvent
      {
        Action = "applytagedit"
      };
      evt.MessageData.Add("tags", (SongData)param);
      EventSystem.Publish(evt);
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
