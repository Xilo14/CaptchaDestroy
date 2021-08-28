using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaptchaDestroy.Core.Interfaces;
using CaptchaDestroy.Web.TgBot.Controls;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types.Payments;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Controls.Inline;
using TelegramBotBase.Form;

namespace CaptchaDestroy.Web.TgBot.Forms
{
    public class DepositForm : BaseForm<StartForm>
    {
        private ButtonBase _createInvoiceBtn;
        private ButtonBase _goToDepositBtn;
        private MessageControl _msg;
        private MultiToggleButton _mtbProviders;
        private MultiToggleButton _mtbCurrencies;
        private MultiToggleButton _mtbAmounts;
        private InvoiceControl _invoice;
        private ProviderInfo _lastProvider;
        public IOptions<TelegramPaymentsConfiguration> PaymentsConfig { get; set; }
        public IOptions<DCPointsAmountsConfiguration> AmountsConfig { get; set; }
        public IAccountService AccountService { get; set; }
        public ICurrencyConverter CurrencyConverter { get; set; }
        public override async Task Initilization(object sender, InitEventArgs args)
        {
            await base.Initilization(sender, args);

            var bf = new ButtonForm();

            _goToDepositBtn = new ButtonBase("", "goback");
            _createInvoiceBtn = new ButtonBase("", "createinvoice");

            bf.AddButtonRow(_createInvoiceBtn);
            bf.AddButtonRow(_goToDepositBtn);

            _msg = new MessageControl(bf);

            _mtbProviders = new MultiToggleButton
            {
                Options = PaymentsConfig.Value.Providers
                    .Select(p => new ButtonBase(p.Name, p.Name))
                    .ToList()
            };
            _mtbProviders.SelectedOption = _mtbProviders.Options.FirstOrDefault();

            _mtbCurrencies = new MultiToggleButton
            {
                Options = PaymentsConfig.Value.Providers
                    .FirstOrDefault(p => p.Name == _mtbProviders.SelectedOption.Value).Currencies
                    .Select(c => new ButtonBase(c, c))
                    .ToList(),
            };
            _mtbCurrencies.SelectedOption = _mtbCurrencies.Options.FirstOrDefault();

            _mtbAmounts = new MultiToggleButton
            {
                Options = AmountsConfig.Value.Amounts
                    .Select(a => new ButtonBase(a.Amount.ToString(), a.Amount.ToString()))
                    .ToList(),

            };
            _mtbAmounts.SelectedOption = _mtbAmounts.Options.FirstOrDefault();

            _invoice = new InvoiceControl()
            {
                Enabled = false,
            };

            await UpdateForm(null);
            AddControl(_msg);
            AddControl(_mtbAmounts);
            AddControl(_mtbProviders);
            AddControl(_mtbCurrencies);
            AddControl(_invoice);
        }
        public override Task UpdateForm(MessageResult message)
        {
            //var account = await AccountService.GetOrCreateAccount(Device.DeviceId);

            _msg.Text = Localizer.GetWithCulture("Choose values and create invoice", Culture);
            _goToDepositBtn.Text = Localizer.GetWithCulture("Go Back", Culture);
            _createInvoiceBtn.Text = Localizer.GetWithCulture("Create Invoice", Culture);

            _mtbProviders.Title = Localizer.GetWithCulture("Choose provider", Culture);
            _mtbAmounts.Title = Localizer.GetWithCulture("Choose amount of DC Points", Culture);
            _mtbCurrencies.Title = Localizer.GetWithCulture("Choose currency", Culture);


            var currentProv = PaymentsConfig.Value.Providers
                        .FirstOrDefault(p => p.Name == _mtbProviders.SelectedOption.Value);
            if (_lastProvider != currentProv)
            {
                _mtbCurrencies.Options = currentProv.Currencies
                        .Select(c => new ButtonBase(c, c))
                        .ToList();
                _mtbCurrencies.SelectedOption = _mtbCurrencies.Options.FirstOrDefault();
                _lastProvider = currentProv;
            }

            return Task.CompletedTask;
        }
        public override async Task Action(MessageResult message)
        {
            if (message.RawData == "goback")
            {
                await NavigateTo<MainForm>();
                return;
            }

            if (message.RawData == "createinvoice")
            {
                _invoice.ProviderToken = PaymentsConfig.Value.Providers
                    .FirstOrDefault(p => p.Name == _mtbProviders.SelectedOption.Value).Token;
                _invoice.Currency = _mtbCurrencies.SelectedOption.Value;
                _invoice.Prices = new();
                var selectedAmountInfo = AmountsConfig.Value.Amounts
                    .FirstOrDefault(a => a.Amount.ToString() == _mtbAmounts.SelectedOption.Value);

                _invoice.Title = Localizer.GetWithCulture("Title", Culture, selectedAmountInfo.Amount);
                _invoice.Description = Localizer.GetWithCulture("Description", Culture, selectedAmountInfo.Amount);

                var priceInDollar = (int)Math.Ceiling(selectedAmountInfo.Amount / (double)AmountsConfig.Value.DCPointsPerDollar * 100);
                var priceInCurrency = await CurrencyConverter.Convert("USD", priceInDollar, _invoice.Currency);
                _invoice.Payload = selectedAmountInfo.Amount.ToString();
                _invoice.Prices.Add(new("Points", priceInCurrency));
                _invoice.Prices.Add(new("Discount",
                     -(int)(selectedAmountInfo.Discount * _invoice.Prices.First().Amount)));
                _invoice.Enabled = true;
            }
            await base.Action(message);
        }
        public override async Task OnPreCheckoutQuery(MessageResult message)
        {
            await Device.API(a => a.AnswerPreCheckoutQueryAsync(message.RawUpdateArgs.Update.PreCheckoutQuery.Id));
            _invoice.Enabled = false;
            await _invoice.Cleanup();
        }
        public override async Task OnSuccessfulPayment(MessageResult message)
        {
            await AccountService.DepositPoints(
                Device.DeviceId, int.Parse(message.RawMessageData.Message.SuccessfulPayment.InvoicePayload));
        }
    }
}