using System;
using System.Collections.Generic;
using System.Text;

namespace WDTA1
{
    class FactoryAccount
    {
        abstract class aAccount
        {
            public double Balance { get; set; }

            public void Deposit(int accountNumber, Double amount)
            {
                try
                {
                    TalkDB dbObj = new TalkDB();

                    String saveNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
                    String logQuery = $"INSERT INTO [Transaction] (TransactionType,AccountNumber,Amount,TransactionTimeUtc) VALUES('D', '{accountNumber}', '{amount}', '{saveNow}'); ";
                    int rowEffect = dbObj.DbUID(logQuery);

                    String updateBalanceQry = $"UPDATE Account SET Balance = Balance + {amount} WHERE AccountNumber = {accountNumber};";
                    rowEffect = dbObj.DbUID(updateBalanceQry);
                }
                catch
                {
                    Console.WriteLine("Something wrong in Deposit");
                }
                
            }

            public void Transfer(int srcAccount, Double amount, int destAccount)
            {
                try
                {
                    TalkDB dbObj = new TalkDB();

                    //1.First Withdraw from Source Account
                    String withdrawQry = $"UPDATE Account SET Balance = Balance - {amount} WHERE AccountNumber = {srcAccount};";
                    int rowEffect = dbObj.DbUID(withdrawQry);

                    //2.Deposit to Destination account
                    String depositQry = $"UPDATE Account SET Balance = Balance + {amount} WHERE AccountNumber = {destAccount};";
                    rowEffect = dbObj.DbUID(depositQry);

                    //3.Make a transaction 
                    String saveNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
                    String logQuery = $"INSERT INTO [Transaction] (TransactionType,AccountNumber,Amount,TransactionTimeUtc, DestinationAccountNumber) VALUES('T', '{srcAccount}', '{amount}', '{saveNow}', '{destAccount}'); ";
                    rowEffect = dbObj.DbUID(logQuery);
                }catch
                {
                    Console.WriteLine("Something wrong in Transfer.");
                }

            }

            public Double GetWithdrawCharge(int srcAccount)
            {
                try
                {
                    TalkDB dbObj = new TalkDB();

                    String query = $"SELECT COUNT(*) as totalTransaction from [Transaction] WHERE AccountNumber = '{srcAccount}' AND totalTransaction > 4";
                    var accountObj = dbObj.DbSel(query);

                    if (accountObj.Length != 0) return 0.10;
                }
                catch
                {
                    Console.WriteLine("Something wrong in Wtihdraw Charge.");
                }
                return 0.0;
            }

            public Double GetTransferCharge(int srcAccount)
            {
                try
                {
                    TalkDB dbObj = new TalkDB();

                    String query = $"SELECT COUNT(*) as totalTransaction from [Transaction] WHERE AccountNumber = '{srcAccount}' AND totalTransaction > 4 ";
                    var accountObj = dbObj.DbSel(query);

                    if (accountObj.Length != 0) return 0.20;
                } catch
                {
                    Console.WriteLine("Something wrong in Get Transfer Charge");
                }
                return 0.0;
            }

            public Double GetMinimumBalance()
            {
                return 0.0;
            }

            public abstract void WithdrawMoney(int accountNumber, double amount);
            public abstract bool SufficientWithdrawBalance(int accountNumber, double amount, Double Balance, Double MinimumBalance);
            public abstract bool SufficientTransferBalance(int accountNumber, double amount, Double Balance, Double MinimumBalance);

        }

        class Savings : aAccount
        {
            public const double MINIMUM_BALANCE = 0.00;
            public override void WithdrawMoney(int accountNumber, double amount)
            {
                try
                {
                    TalkDB dbObj = new TalkDB();

                    String saveNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
                    String logQuery = $"INSERT INTO [Transaction] (TransactionType,AccountNumber,Amount,TransactionTimeUtc) VALUES('W', '{accountNumber}', '{amount}', '{saveNow}'); ";
                    int rowEffect = dbObj.DbUID(logQuery);

                    String updateBalanceQry = $"UPDATE Account SET Balance = Balance - {amount} WHERE AccountNumber = {accountNumber};";
                    rowEffect = dbObj.DbUID(updateBalanceQry);
                }catch
                {
                    Console.WriteLine("Something wrong in Withdraw Money");
                }
            }

            public override bool SufficientWithdrawBalance(int accountNumber, Double amount, Double Balance, Double MinimumBalance)
            {
                Double withdrawCharge = GetWithdrawCharge(accountNumber);

                if (Balance < (amount + withdrawCharge + MinimumBalance)) return false;
                return true;
            }

            public override bool SufficientTransferBalance(int accountNumber, Double amount, Double Balance, Double MinimumBalance)
            {
                Double TransferCharge = GetTransferCharge(accountNumber);

                if (Balance < (amount + TransferCharge + MinimumBalance)) return false;
                return true;
            }
        }

        class Checking : aAccount
        {
            public const double MINIMUM_BALANCE = 200.00;
            public override void WithdrawMoney(int accountNumber, double amount)
            {
                int rowEffect;
                try
                {
                    TalkDB dbObj = new TalkDB();

                    String saveNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
                    String logQuery = $"INSERT INTO [Transaction] (TransactionType,AccountNumber,Amount,TransactionTimeUtc) VALUES('W', '{accountNumber}', '{amount}', '{saveNow}'); ";
                    rowEffect = dbObj.DbUID(logQuery);

                    String updateBalanceQry = $"UPDATE Account SET Balance = Balance - {amount} WHERE AccountNumber = {accountNumber};";
                    rowEffect = dbObj.DbUID(updateBalanceQry);
                }catch
                {
                    Console.WriteLine("Something wrong in Withdraw");
                }
            }

            public Double GetMinimumBalance()
            {
                return MINIMUM_BALANCE;
            }

            public override bool SufficientWithdrawBalance(int accountNumber, Double amount, Double Balance, Double MinimumBalance)
            {
                Double withdrawCharge = GetWithdrawCharge(accountNumber);

                if (Balance < (amount + withdrawCharge + MinimumBalance)) return false;
                return true;
            }

            public override bool SufficientTransferBalance(int accountNumber, Double amount, Double Balance, Double MinimumBalance)
            {
                Double TransferCharge = GetTransferCharge(accountNumber);

                if (Balance < (amount + TransferCharge + MinimumBalance)) return false;
                return true;
            }


            public bool GetTransferWithCharge(int accountNumber)
            {
                Double TransferCharge = GetTransferCharge(accountNumber);
                return true;
            }
        }

        /* This class is used for proxy Pattern
         * 
         * */
        class ProxyChecking : aAccount
        {
            public const double MINIMUM_BALANCE = 200.00;
            Checking checkingObj = new Checking();
            public override void WithdrawMoney(int accountNumber, double amount)
            {
                try
                {
                    TalkDB dbObj = new TalkDB();

                    String saveNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
                    String logQuery = $"INSERT INTO [Transaction] (TransactionType,AccountNumber,Amount,TransactionTimeUtc) VALUES('W', '{accountNumber}', '{amount}', '{saveNow}'); ";
                    int rowEffect = dbObj.DbUID(logQuery);

                    String updateBalanceQry = $"UPDATE Account SET Balance = Balance - {amount} WHERE AccountNumber = {accountNumber};";
                    rowEffect = dbObj.DbUID(updateBalanceQry);
                }catch
                {
                    Console.WriteLine("Something Wrong in Withdraw money.");
                }
            }

            public Double GetMinimumBalance()
            {
                return checkingObj.GetMinimumBalance();
                //return 200.0;
            }

            public override bool SufficientWithdrawBalance(int accountNumber, Double amount, Double Balance, Double MinimumBalance)
            {
                Double withdrawCharge = GetWithdrawCharge(accountNumber);

                if (Balance < (amount + withdrawCharge + MinimumBalance)) return false;
                return true;
            }

            public override bool SufficientTransferBalance(int accountNumber, Double amount, Double Balance, Double MinimumBalance)
            {
                Double TransferCharge = GetTransferCharge(accountNumber);

                if (Balance < (amount + TransferCharge + MinimumBalance)) return false;
                return true;
            }


            public bool GetTransferWithCharge(int accountNumber)
            {
                Double TransferCharge = GetTransferCharge(accountNumber);
                return true;
            }

        }

        static class Factory
        {
            public static aAccount GetAccountType(String accountType)
            {
                switch (accountType)
                {
                    case "C":
                        return new Checking();
                    case "S":
                        return new Savings();
                    default:
                        return new Checking();
                }
            }

        }

        public void Deposit(int accountNumber, Double amount, String accountType)
        {
            var accountObj = Factory.GetAccountType(accountType);
            accountObj.Deposit(accountNumber, amount);
        }


        public void Withdraw(int accountNumber, Double amount, String accountType)
        {
            var accountObj = Factory.GetAccountType(accountType);
            accountObj.WithdrawMoney(accountNumber, amount);
        }

        public void Transfer(int srcAccountNumber, Double amount, int destAccountNumber)
        {
            var accountObj = Factory.GetAccountType("");
            accountObj.Transfer(srcAccountNumber, amount, destAccountNumber);
        }

        public bool IsSufficientWithdrawBalance(int srcAccountNumber, Double amount, String accountType, Double Balance)
        {
            var accountObj = Factory.GetAccountType(accountType);
            Double MinimumBalance;
            if (accountType == "C")
            {
                var proxyCheckObj = new ProxyChecking();
                MinimumBalance = proxyCheckObj.GetMinimumBalance();
            } else
            {
                MinimumBalance = 0.0;
            }

            return accountObj.SufficientWithdrawBalance(srcAccountNumber, amount, Balance, MinimumBalance);
        }

        public bool IsSufficientTransferBalance(int srcAccountNumber, Double amount, String accountType, Double Balance)
        {
            Double MinimumBalance;
            if (accountType == "C")
            {
                var proxyCheckObj = new ProxyChecking();
                MinimumBalance = proxyCheckObj.GetMinimumBalance();
            }
            else
            {
                MinimumBalance = 0.0;
            }

            var accountObj = Factory.GetAccountType(accountType);
            return accountObj.SufficientWithdrawBalance(srcAccountNumber, amount, Balance, MinimumBalance);
        }
    }
}
