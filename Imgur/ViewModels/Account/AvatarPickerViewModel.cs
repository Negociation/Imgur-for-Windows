using Imgur.Api.Services.Models.Enum;
using Imgur.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Imgur.ViewModels.Account
{
    public class AvatarPickerItem
    {
        public ImgurAvatar Avatar { get; set; }
        public string DisplayName { get; set; }
        public string ImagePath { get; set; }
        public string ApiValue { get; set; }
    }

    public class AvatarPickerViewModel : Observable
    {
        private AvatarPickerItem _selectedItem;

        public AvatarPickerViewModel(ImgurAvatar? current)
        {
            Items = new ObservableCollection<AvatarPickerItem>();
            foreach (ImgurAvatar avatar in Enum.GetValues(typeof(ImgurAvatar)))
            {
                var item = new AvatarPickerItem
                {
                    Avatar = avatar,
                    DisplayName = avatar.GetSlug(),
                    ImagePath = avatar.GetLocalImagePath(),
                    ApiValue = avatar.GetApiValue()
                };
                Items.Add(item);
                if (current.HasValue && current.Value.Equals(avatar))
                    SelectedItem = item;
            }
        }

        public ObservableCollection<AvatarPickerItem> Items { get; }

        public AvatarPickerItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (ReferenceEquals(_selectedItem, value)) return;
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(HasSelection));
            }
        }

        public bool HasSelection => SelectedItem != null;

        private ICommand _selectCommand;
        public ICommand SelectCommand
        {
            get
            {
                if (_selectCommand == null)
                {
                    _selectCommand = new RelayCommand<AvatarPickerItem>(item =>
                    {
                        if (item != null)
                            SelectedItem = item;
                    });
                }
                return _selectCommand;
            }
        }
    }
}
