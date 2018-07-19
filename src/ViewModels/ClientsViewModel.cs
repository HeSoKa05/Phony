﻿using LiteDB;
using MahApps.Metro.Controls.Dialogs;
using Phony.Kernel;
using Phony.Models;
using Phony.Views;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Phony.ViewModels
{
    public class ClientsViewModel : BindableBase
    {
        long _clientsId;
        string _name;
        string _site;
        string _email;
        string _phone;
        string _notes;
        string _searchText;
        string _childName;
        string _childPrice;
        static string _clientsCount;
        static string _clientsPurchasePrice;
        static string _clientsSalePrice;
        static string _clientsProfit;
        decimal _balance;
        bool _fastResult;
        bool _openFastResult;
        bool _isAddClientFlyoutOpen;
        Client _dataGridSelectedClient;

        ObservableCollection<Client> _clients;

        public long ClientId
        {
            get => _clientsId;
            set => SetProperty(ref _clientsId, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string Site
        {
            get => _site;
            set => SetProperty(ref _site, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string ChildName
        {
            get => _childName;
            set => SetProperty(ref _childName, value);
        }

        public string ChildPrice
        {
            get => _childPrice;
            set => SetProperty(ref _childPrice, value);
        }

        public string ClientCount
        {
            get => _clientsCount;
            set => SetProperty(ref _clientsCount, value);
        }

        public string ClientCredits
        {
            get => _clientsPurchasePrice;
            set => SetProperty(ref _clientsPurchasePrice, value);
        }

        public string ClientDebits
        {
            get => _clientsSalePrice;
            set => SetProperty(ref _clientsSalePrice, value);
        }

        public string ClientProfit
        {
            get => _clientsProfit;
            set => SetProperty(ref _clientsProfit, value);
        }

        public decimal Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        public bool FastResult
        {
            get => _fastResult;
            set => SetProperty(ref _fastResult, value);
        }

        public bool OpenFastResult
        {
            get => _openFastResult;
            set => SetProperty(ref _openFastResult, value);
        }

        public bool IsAddClientFlyoutOpen
        {
            get => _isAddClientFlyoutOpen;
            set => SetProperty(ref _isAddClientFlyoutOpen, value);
        }

        public Client DataGridSelectedClient
        {
            get => _dataGridSelectedClient;
            set => SetProperty(ref _dataGridSelectedClient, value);
        }

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set => SetProperty(ref _clients, value);
        }

        public ObservableCollection<User> Users { get; set; }

        public ICommand ClientPay { get; set; }
        public ICommand PayClient { get; set; }
        public ICommand AddClient { get; set; }
        public ICommand EditClient { get; set; }
        public ICommand DeleteClient { get; set; }
        public ICommand Search { get; set; }
        public ICommand OpenAddClientFlyout { get; set; }
        public ICommand FillUI { get; set; }
        public ICommand ReloadAllClients { get; set; }

        Clients ClientsMessage = Application.Current.Windows.OfType<Clients>().FirstOrDefault();

        public ClientsViewModel()
        {
            LoadCommands();
            using (var db = new LiteDatabase(Properties.Settings.Default.DBFullName))
            {
                Clients = new ObservableCollection<Client>(db.GetCollection<Client>(Data.DBCollections.Clients).FindAll());
                Users = new ObservableCollection<User>(db.GetCollection<User>(Data.DBCollections.Users).FindAll());
            }
            DebitCredit();
        }

        public void LoadCommands()
        {
            ClientPay = new DelegateCommand(DoClientPayAsync, CanClientPay).ObservesProperty(() => DataGridSelectedClient);
            PayClient = new DelegateCommand(DoPayClientAsync, CanPayClient).ObservesProperty(() => DataGridSelectedClient);
            AddClient = new DelegateCommand(DoAddClient, CanAddClient).ObservesProperty(() => Name);
            EditClient = new DelegateCommand(DoEditClient, CanEditClient).ObservesProperty(() => Name).ObservesProperty(() => ClientId).ObservesProperty(() => DataGridSelectedClient);
            DeleteClient = new DelegateCommand(DoDeleteClient, CanDeleteClient).ObservesProperty(() => DataGridSelectedClient).ObservesProperty(() => DataGridSelectedClient.Id);
            Search = new DelegateCommand(DoSearch, CanSearch).ObservesProperty(() => SearchText);
            OpenAddClientFlyout = new DelegateCommand(DoOpenAddClientFlyout, CanOpenAddClientFlyout);
            FillUI = new DelegateCommand(DoFillUI, CanFillUI).ObservesProperty(() => DataGridSelectedClient);
            ReloadAllClients = new DelegateCommand(DoReloadAllClients, CanReloadAllClients);
        }

        async void DebitCredit()
        {
            decimal Debit = decimal.Round(Clients.Where(c => c.Balance > 0).Sum(i => i.Balance), 2);
            decimal Credit = decimal.Round(Clients.Where(c => c.Balance < 0).Sum(i => i.Balance), 2);
            await Task.Run(() =>
            {
                ClientCount = $"مجموع العملاء: {Clients.Count().ToString()}";
            });
            await Task.Run(() =>
            {
                ClientDebits = $"اجمالى لينا: {Math.Abs(Debit).ToString()}";
            });
            await Task.Run(() =>
            {
                ClientCredits = $"اجمالى علينا: {Math.Abs(Credit).ToString()}";
            });
            await Task.Run(() =>
            {
                ClientProfit = $"تقدير لصافى لينا: {(Math.Abs(Debit) - Math.Abs(Credit)).ToString()}";
            });
        }

        private bool CanClientPay()
        {
            if (DataGridSelectedClient == null)
            {
                return false;
            }
            return true;
        }

        private async void DoClientPayAsync()
        {
            var result = await ClientsMessage.ShowInputAsync("تدفيع", $"ادخل المبلغ الذى تريد خصمه من حساب العميل {DataGridSelectedClient.Name}");
            if (string.IsNullOrWhiteSpace(result))
            {
                await ClientsMessage.ShowMessageAsync("ادخل مبلغ", "لم تقم بادخال اى مبلغ لتدفيعه");
            }
            else
            {
                bool isvalidmoney = decimal.TryParse(result, out decimal clientpaymentamount);
                if (isvalidmoney)
                {
                    using (var db = new LiteDatabase(Properties.Settings.Default.DBFullName))
                    {
                        var c = db.GetCollection<Client>(Data.DBCollections.Clients.ToString()).FindById(DataGridSelectedClient.Id);
                        c.Balance -= clientpaymentamount;
                        db.GetCollection<ClientMove>(Data.DBCollections.ClientsMoves.ToString()).Insert(new ClientMove
                        {
                            Client = db.GetCollection<Client>(Data.DBCollections.Clients.ToString()).FindById(DataGridSelectedClient.Id),
                            Credit = clientpaymentamount,
                            CreateDate = DateTime.Now,
                            Creator = Core.ReadUserSession(),
                            EditDate = null,
                            Editor = null
                        });
                        if (clientpaymentamount > 0)
                        {
                            db.GetCollection<TreasuryMove>(Data.DBCollections.TreasuriesMoves.ToString()).Insert(new TreasuryMove
                            {
                                Treasury = db.GetCollection<Treasury>(Data.DBCollections.Treasuries.ToString()).FindById(1),
                                Debit = clientpaymentamount,
                                Notes = $"استلام من العميل بكود {DataGridSelectedClient.Id} باسم {DataGridSelectedClient.Name}",
                                CreateDate = DateTime.Now,
                                Creator = Core.ReadUserSession(),
                                EditDate = null,
                                Editor = null
                            });
                        }
                        await ClientsMessage.ShowMessageAsync("تمت العملية", $"تم تدفيع {DataGridSelectedClient.Name} مبلغ {clientpaymentamount} جنية بنجاح");
                        Clients[Clients.IndexOf(DataGridSelectedClient)] = c;
                        DebitCredit();
                        ClientId = 0;
                        DataGridSelectedClient = null;
                    }
                }
                else
                {
                    await ClientsMessage.ShowMessageAsync("خطاء فى المبلغ", "ادخل مبلغ صحيح بعلامه عشرية واحدة");
                }
            }
        }

        private bool CanPayClient()
        {
            if (DataGridSelectedClient == null)
            {
                return false;
            }
            return true;
        }

        private async void DoPayClientAsync()
        {
            var result = await ClientsMessage.ShowInputAsync("تدفيع", $"ادخل المبلغ الذى تريد اضافته لحساب للعميل {DataGridSelectedClient.Name}");
            if (string.IsNullOrWhiteSpace(result))
            {
                await ClientsMessage.ShowMessageAsync("ادخل مبلغ", "لم تقم بادخال اى مبلغ لتدفيعه");
            }
            else
            {
                bool isvalidmoney = decimal.TryParse(result, out decimal clientpaymentamount);
                if (isvalidmoney)
                {
                    using (var db = new LiteDatabase(Properties.Settings.Default.DBFullName))
                    {
                        var c = db.GetCollection<Client>(Data.DBCollections.Clients.ToString()).FindById(DataGridSelectedClient.Id);
                        c.Balance += clientpaymentamount;
                        db.GetCollection<ClientMove>(Data.DBCollections.ClientsMoves.ToString()).Insert(new ClientMove
                        {
                            Client = db.GetCollection<Client>(Data.DBCollections.Clients.ToString()).FindById(DataGridSelectedClient.Id),
                            Debit = clientpaymentamount,
                            CreateDate = DateTime.Now,
                            Creator = Core.ReadUserSession(),
                            EditDate = null,
                            Editor = null
                        });
                        db.GetCollection<TreasuryMove>(Data.DBCollections.TreasuriesMoves.ToString()).Insert(new TreasuryMove
                        {
                            Treasury = db.GetCollection<Treasury>(Data.DBCollections.Treasuries.ToString()).FindById(1),
                            Credit = clientpaymentamount,
                            Notes = $"تسليم المبلغ للعميل بكود {DataGridSelectedClient.Id} باسم {DataGridSelectedClient.Name}",
                            CreateDate = DateTime.Now,
                            Creator = Core.ReadUserSession(),
                            EditDate = null,
                            Editor = null
                        });
                        await ClientsMessage.ShowMessageAsync("تمت العملية", $"تم دفع {DataGridSelectedClient.Name} مبلغ {clientpaymentamount} جنية بنجاح");
                        Clients[Clients.IndexOf(DataGridSelectedClient)] = c;
                        DebitCredit();
                        ClientId = 0;
                        DataGridSelectedClient = null;
                    }
                }
                else
                {
                    await ClientsMessage.ShowMessageAsync("خطاء فى المبلغ", "ادخل مبلغ صحيح بعلامه عشرية واحدة");
                }
            }
        }

        private bool CanAddClient()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return false;
            }
            return true;
        }

        private void DoAddClient()
        {
            using (var db = new LiteDatabase(Properties.Settings.Default.DBFullName))
            {
                var exist = db.GetCollection<Client>(Data.DBCollections.Clients.ToString()).Find(x => x.Name == Name).FirstOrDefault();
                if (exist == null)
                {
                    var c = new Client
                    {
                        Name = Name,
                        Balance = Balance,
                        Site = Site,
                        Email = Email,
                        Phone = Phone,
                        Notes = Notes,
                        CreateDate = DateTime.Now,
                        Creator = Core.ReadUserSession(),
                        EditDate = null,
                        Editor = null
                    };
                    db.GetCollection<Client>(Data.DBCollections.Clients.ToString()).Insert(c);
                    Clients.Add(c);
                    ClientsMessage.ShowMessageAsync("تمت العملية", "تم اضافة العميل بنجاح");
                    DebitCredit();
                }
                else
                {
                    ClientsMessage.ShowMessageAsync("موجود", "هناك عميل بنفس الاسم بالفعل");
                }
            }
        }

        private bool CanEditClient()
        {
            if (string.IsNullOrWhiteSpace(Name) || ClientId == 0 || DataGridSelectedClient == null)
            {
                return false;
            }
            return true;
        }

        private void DoEditClient()
        {
            using (var db = new LiteDatabase(Properties.Settings.Default.DBFullName))
            {
                var c = db.GetCollection<Client>(Data.DBCollections.Clients.ToString()).FindById(DataGridSelectedClient.Id);
                c.Name = Name;
                c.Balance = Balance;
                c.Site = Site;
                c.Email = Email;
                c.Phone = Phone;
                c.Notes = Notes;
                c.Editor = Core.ReadUserSession();
                c.EditDate = DateTime.Now;
                db.GetCollection<Client>(Data.DBCollections.Clients.ToString()).Update(c);
                ClientsMessage.ShowMessageAsync("تمت العملية", "تم تعديل العميل بنجاح");
                Clients[Clients.IndexOf(DataGridSelectedClient)] = c;
                DebitCredit();
                DataGridSelectedClient = null;
                ClientId = 0;
            }
        }

        private bool CanDeleteClient()
        {
            if (DataGridSelectedClient == null || DataGridSelectedClient.Id == 1)
            {
                return false;
            }
            return true;
        }

        private async void DoDeleteClient()
        {
            var result = await ClientsMessage.ShowMessageAsync("حذف الصنف", $"هل انت متاكد من حذف العميل {DataGridSelectedClient.Name}", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                using (var db = new LiteDatabase(Properties.Settings.Default.DBFullName))
                {
                    db.GetCollection<Client>(Data.DBCollections.Clients.ToString()).Delete(DataGridSelectedClient.Id);
                    Clients.Remove(DataGridSelectedClient);
                }
                await ClientsMessage.ShowMessageAsync("تمت العملية", "تم حذف العميل بنجاح");
                DebitCredit();
                DataGridSelectedClient = null;
            }
        }

        private bool CanSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return false;
            }
            return true;
        }

        private void DoSearch()
        {
            try
            {
                using (var db = new LiteDatabase(Properties.Settings.Default.DBFullName))
                {
                    Clients = new ObservableCollection<Client>(db.GetCollection<Client>(Data.DBCollections.Clients.ToString()).Find(x => x.Name.Contains(SearchText)));
                    if (Clients.Count > 0)
                    {
                        if (FastResult)
                        {
                            ChildName = Clients.FirstOrDefault().Name;
                            ChildPrice = Clients.FirstOrDefault().Balance.ToString();
                            OpenFastResult = true;
                        }
                    }
                    else
                    {
                        ClientsMessage.ShowMessageAsync("غير موجود", "لم يتم العثور على شئ");
                    }
                }
            }
            catch (Exception ex)
            {
                Core.SaveException(ex);
                BespokeFusion.MaterialMessageBox.ShowError("لم يستطع ايجاد ما تبحث عنه تاكد من صحه البيانات المدخله");
            }
        }

        private bool CanReloadAllClients()
        {
            return true;
        }

        private void DoReloadAllClients()
        {
            using (var db = new LiteDatabase(Properties.Settings.Default.DBFullName))
            {
                Clients = new ObservableCollection<Client>(db.GetCollection<Client>(Data.DBCollections.Clients).FindAll());
            }
        }

        private bool CanFillUI()
        {
            if (DataGridSelectedClient == null)
            {
                return false;
            }
            return true;
        }

        private void DoFillUI()
        {
            ClientId = DataGridSelectedClient.Id;
            Name = DataGridSelectedClient.Name;
            Balance = DataGridSelectedClient.Balance;
            Site = DataGridSelectedClient.Site;
            Email = DataGridSelectedClient.Email;
            Phone = DataGridSelectedClient.Phone;
            Notes = DataGridSelectedClient.Notes;
            IsAddClientFlyoutOpen = true;
        }

        private bool CanOpenAddClientFlyout()
        {
            return true;
        }

        private void DoOpenAddClientFlyout()
        {
            if (IsAddClientFlyoutOpen)
            {
                IsAddClientFlyoutOpen = false;
            }
            else
            {
                IsAddClientFlyoutOpen = true;
            }
        }
    }
}