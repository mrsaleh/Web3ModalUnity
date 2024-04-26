using UnityEngine;
using UnityEngine.UIElements;
using WalletConnect.UI;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;

namespace WalletConnect.Web3Modal
{
    public class QrCodePresenter : Presenter<QrCodeView>
    {
        public override string Title
        {
            get => "QR Code";
        }

        private WalletConnectConnectionProposal _connectionProposal;

        public QrCodePresenter(RouterController router, VisualElement parent, bool hide = true) : base(router)
        {
            View = CreateVisualElement(parent);
            if (hide)
                View.style.display = DisplayStyle.None;
        }

        public QrCodePresenter(RouterController router, QrCodeView qrCodeView) : base(router)
        {
            View = qrCodeView;

            qrCodeView.copyLink.Clicked += OnCopyLinkClicked;
        }

        private QrCodeView CreateVisualElement(VisualElement parent)
        {
            var view = new QrCodeView();

            view.copyLink.Clicked += OnCopyLinkClicked;

            parent.Add(view);
            return view;
        }

        protected override void OnVisibleCore()
        {
            base.OnVisibleCore();

            if (!Web3Modal.ConnectorController
                    .TryGetConnector<WalletConnectConnector>
                        (ConnectorType.WalletConnect, out var connector))
                throw new System.Exception("No WC connector"); // TODO: use custom exception

            if (_connectionProposal == null || _connectionProposal.IsConnected)
            {
                _connectionProposal = (WalletConnectConnectionProposal)connector.Connect();
                _connectionProposal.ConnectionUpdated += OnConnectionProposalUpdated;
            }

            View.qrCode.Data = _connectionProposal.Uri;

            if (WalletUtils.TryGetLastViewedWallet(out var wallet))
            {
                var remoteSprite = RemoteSpriteFactory.GetRemoteSprite<Image>($"https://api.web3modal.com/getWalletImage/{wallet.ImageId}");
                View.EnableWalletIcon(remoteSprite);
            }
            else
            {
                View.DisableWalletIcon();
            }
        }

        private void OnCopyLinkClicked()
        {
            GUIUtility.systemCopyBuffer = _connectionProposal.Uri;
            Web3Modal.NotificationController.Notify(NotificationType.Success, "Link copied to clipboard");
        }

        private void OnConnectionProposalUpdated(ConnectionProposal connectionProposal)
        {
            View.qrCode.Data = _connectionProposal.Uri;
        }
    }
}