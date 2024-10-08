using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.IO;
using System.Reactive;
using v2rayN.Base;
using v2rayN.Enums;
using v2rayN.Handler;
using v2rayN.Models;
using v2rayN.Resx;

namespace v2rayN.ViewModels
{
    public class AddServer2ViewModel : MyReactiveObject
    {
        [Reactive]
        public ProfileItem SelectedSource { get; set; }

        public ReactiveCommand<Unit, Unit> BrowseServerCmd { get; }
        public ReactiveCommand<Unit, Unit> EditServerCmd { get; }
        public ReactiveCommand<Unit, Unit> SaveServerCmd { get; }
        public bool IsModified { get; set; }

        public AddServer2ViewModel(ProfileItem profileItem, Func<EViewAction, object?, bool>? updateView)
        {
            _noticeHandler = Locator.Current.GetService<NoticeHandler>();
            _config = LazyConfig.Instance.GetConfig();
            _updateView = updateView;

            if (profileItem.indexId.IsNullOrEmpty())
            {
                SelectedSource = profileItem;
            }
            else
            {
                SelectedSource = JsonUtils.DeepCopy(profileItem);
            }

            BrowseServerCmd = ReactiveCommand.Create(() =>
            {
                _updateView?.Invoke(EViewAction.BrowseServer, null);
            });

            EditServerCmd = ReactiveCommand.Create(() =>
            {
                EditServer();
            });

            SaveServerCmd = ReactiveCommand.Create(() =>
            {
                SaveServer();
            });
        }

        private void SaveServer()
        {
            string remarks = SelectedSource.remarks;
            if (Utils.IsNullOrEmpty(remarks))
            {
                _noticeHandler?.Enqueue(ResUI.PleaseFillRemarks);
                return;
            }

            if (Utils.IsNullOrEmpty(SelectedSource.address))
            {
                _noticeHandler?.Enqueue(ResUI.FillServerAddressCustom);
                return;
            }

            var item = LazyConfig.Instance.GetProfileItem(SelectedSource.indexId);
            if (item is null)
            {
                item = SelectedSource;
            }
            else
            {
                item.remarks = SelectedSource.remarks;
                item.address = SelectedSource.address;
                item.coreType = SelectedSource.coreType;
                item.displayLog = SelectedSource.displayLog;
                item.preSocksPort = SelectedSource.preSocksPort;
            }

            if (ConfigHandler.EditCustomServer(_config, item) == 0)
            {
                _noticeHandler?.Enqueue(ResUI.OperationSuccess);
                _updateView?.Invoke(EViewAction.CloseWindow, null);
            }
            else
            {
                _noticeHandler?.Enqueue(ResUI.OperationFailed);
            }
        }

        public void BrowseServer(string fileName)
        {
            if (Utils.IsNullOrEmpty(fileName))
            {
                return;
            }

            var item = LazyConfig.Instance.GetProfileItem(SelectedSource.indexId);
            item ??= SelectedSource;
            item.address = fileName;
            if (ConfigHandler.AddCustomServer(_config, item, false) == 0)
            {
                _noticeHandler?.Enqueue(ResUI.SuccessfullyImportedCustomServer);
                if (!Utils.IsNullOrEmpty(item.indexId))
                {
                    SelectedSource = JsonUtils.DeepCopy(item);
                }
                IsModified = true;
            }
            else
            {
                _noticeHandler?.Enqueue(ResUI.FailedImportedCustomServer);
            }
        }

        private void EditServer()
        {
            var address = SelectedSource.address;
            if (Utils.IsNullOrEmpty(address))
            {
                _noticeHandler?.Enqueue(ResUI.FillServerAddressCustom);
                return;
            }

            address = Utils.GetConfigPath(address);
            if (File.Exists(address))
            {
                Utils.ProcessStart(address);
            }
            else
            {
                _noticeHandler?.Enqueue(ResUI.FailedReadConfiguration);
            }
        }
    }
}