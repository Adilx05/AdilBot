using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Timers;
using System.Web.Script.Serialization;
using Ionic.Zip;
using Microsoft.Win32;
using SteamKit2.Unified.Internal;
using Timer = System.Timers.Timer;

namespace SteamBot
{
    class Program
    {
        static string user, pass,mailId,mailPass,receiverMail;

        static SteamClient steamClient;
        static CallbackManager manager;
        static SteamUser steamUser;
        static SteamFriends steamFriends;
        static bool isRunning = false;
        static string authCode;

        static void Main(string[] args)
        {
            int saniye = 1000;
            int dakika = 60000;
            int saat = 3600000;

            Timer tm = new Timer();
            tm.Interval = 5 * saat;
            tm.Start();
            tm.Elapsed += TmOnElapsed;
            DeleteTemps();
            Update();

            if (!File.Exists("admin.txt"))
            {
                File.Create("admin.txt").Close();
                File.WriteAllText("admin.txt", "76561198226166664");
            }

            Console.Title = "Bot Hazırlanıyor... ";
            Console.WriteLine("Programdan Çıkmak İçin : CTRL+C");

            #region UserPassSaving

            if (Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("AdilBot")?.GetValue("AccID").ToString() != null)
            {
                user = Registry.CurrentUser.CreateSubKey("Software")
                    ?.OpenSubKey("AdilBot")
                    ?.GetValue("AccID")
                    .ToString();
                pass = Registry.CurrentUser.CreateSubKey("Software")
                    ?.OpenSubKey("AdilBot")
                    ?.GetValue("AccPass")
                    .ToString();
            }
            else
            {
                Console.WriteLine("Kullanıcı Adı Ve Parolanızı Kaydetmek İster Misiniz ?  E / H ?");
                if (Console.ReadLine() == "E" || Console.ReadLine() == "e")
                {
                    Console.Write("Kullanıcı Adı : ");
                    user = Console.ReadLine();
                    Registry.CurrentUser.CreateSubKey("Software")?.CreateSubKey("AdilBot")?.SetValue("AccID", user);
                    Console.Write("Parola : ");
                    pass = Console.ReadLine();
                    Registry.CurrentUser.CreateSubKey("Software")
                        ?.CreateSubKey("AdilBot")
                        ?.SetValue("AccPass", pass);
                }
                else
                {
                    Console.Write("Kullanıcı Adı: ");
                    user = Console.ReadLine();
                    Console.Write("Parola: ");
                    pass = Console.ReadLine();
                }

            }

            #endregion

            #region MailSaving
            //if (Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("AdilBot")?.GetValue("AccID").ToString() != null)
            if (Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("AdilBot")?.GetValue("MailID")?.ToString() != null)
            {
                mailId = Registry.CurrentUser.CreateSubKey("Software")
                    ?.OpenSubKey("AdilBot")
                    ?.GetValue("MailID")
                    .ToString();
                mailPass = Registry.CurrentUser.CreateSubKey("Software")
                    ?.OpenSubKey("AdilBot")
                    ?.GetValue("MailPass")
                    .ToString();
                receiverMail = Registry.CurrentUser.CreateSubKey("Software")
                    ?.OpenSubKey("AdilBot")
                    ?.GetValue("ReceiverMail")
                    .ToString();
            }
            else
            {
                Console.WriteLine("Mail Fonksiyonunu Kullanmak İster Misiniz (Sadece Yandex Desteklenmektedir) ?  E / H ?");
                if (Console.ReadLine() == "E" || Console.ReadLine() == "e")
                {
                    Console.Write("Gönderici Mail Adresi (Mail Göndermek İçin Kullanılacak) : ");
                    mailId = Console.ReadLine();
                    Registry.CurrentUser.CreateSubKey("Software")?.CreateSubKey("AdilBot")?.SetValue("MailID", mailId);
                    Console.Write("Parola : ");
                    mailPass = Console.ReadLine();
                    Registry.CurrentUser.CreateSubKey("Software")
                        ?.CreateSubKey("AdilBot")
                        ?.SetValue("MailPass", mailPass);

                    Console.Write("Alıcı Mail Adresi (Mail Bu Adrese Gönderilecek) : ");
                    receiverMail = Console.ReadLine();
                    Registry.CurrentUser.CreateSubKey("Software")?.CreateSubKey("AdilBot")?.SetValue("ReceiverMail", receiverMail);
                }

            }
            #endregion



            SteamLogIn();

        }

        public static void Update()
        {
            var wc = new WebClient { Proxy = null };
            try
            {
                var guncelleme = wc.DownloadString("https://raw.githubusercontent.com/Adilx05/AdilBot/master/Version");
                if (guncelleme != "106")
                {
                    using (var client = new WebClient())
                    {
                        Console.WriteLine("Güncelleniyor ...");
                        client.DownloadFile(
                            "https://github.com/Adilx05/AdilBot/raw/master/AdilBot.zip",
                            "AdilBot.zip");
                        
                        string cikarilacak = "AdilBot.zip";
                        using (ZipFile zip1 = ZipFile.Read(cikarilacak))
                        {
                            foreach (ZipEntry s in zip1)
                            {
                                s.Extract("Temp", ExtractExistingFileAction.OverwriteSilently);
                            }
                        }
                        Console.WriteLine("Güncelleme Tamamlandı. Yeniden Başlatılıyor...");
                    }

                    Process tasi = new Process();
                    tasi.StartInfo.FileName = "cmd.exe";
                    tasi.StartInfo.Arguments = "/C timeout /t 4 & XCOPY Temp\\* /y";
                    tasi.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    tasi.Start();

                    Process restartla = new Process();
                    restartla.StartInfo.FileName = "cmd.exe";
                    restartla.StartInfo.Arguments = "/C timeout /t 10 & START SteamBot.exe ";
                    restartla.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    restartla.Start();

                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }

        private static void TmOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Update();
        }

        public static void DeleteTemps()
        {
            Process sil = new Process();
            sil.StartInfo.FileName = "cmd.exe";
            sil.StartInfo.Arguments = "/C RMDIR \"Temp\" /S /Q ";
            sil.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            sil.Start();

            Process sil2 = new Process();
            sil2.StartInfo.FileName = "cmd.exe";
            sil2.StartInfo.Arguments = "/C DEL AdilBot.zip /S /Q";
            sil2.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            sil2.Start();
        }

        static void SteamLogIn()
        {
            steamClient = new SteamClient();
            manager = new CallbackManager(steamClient);
            steamUser = steamClient.GetHandler<SteamUser>();

            steamFriends = steamClient.GetHandler<SteamFriends>();

            new Callback<SteamClient.ConnectedCallback>(OnConnected, manager);

            new Callback<SteamUser.LoggedOnCallback>(OnLoggedOn, manager);

            new Callback<SteamUser.AccountInfoCallback>(OnAccountInfo, manager);

            new Callback<SteamFriends.FriendMsgCallback>(OnChatMessage, manager);

            new Callback<SteamFriends.FriendsListCallback>(OnFriendLists, manager);

            new Callback<SteamClient.DisconnectedCallback>(OnDisconnected, manager);

            new Callback<SteamUser.UpdateMachineAuthCallback>(UpdateMachineAuthCallback, manager);

            new Callback<SteamFriends.ChatInviteCallback>(OnChatInvite, manager);

            new Callback<SteamFriends.ChatEnterCallback>(OnChatEnter, manager);

            new Callback<SteamFriends.ChatMsgCallback>(OnGrupMsg, manager);

            isRunning = true;

            Console.WriteLine(DateTime.Now + "  Steam'e Bağlanılıyor...\n");

            steamClient.Connect();

            var simdi = DateTime.Now;
            
            while (isRunning)
            {
                var sure = DateTime.Now - simdi;
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                Console.Title = "Bot Aktif : " + steamFriends.GetPersonaName() + "      Geçen Zaman : " + sure.Days + " Gün " + sure.Hours + ":" + sure.Minutes + ":" + sure.Seconds;
            }
            Console.ReadKey();
        }

        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine(DateTime.Now+"  Steam'e Bağlanılamadı{0}", callback.Result);
                isRunning = false;
                return;
            }
            Console.WriteLine(DateTime.Now + "  Steam'e Bağlanılıyor. \n{1}  Giriş Yapılıyor {0}...\n", user,DateTime.Now);

            byte[] sentryHash = null;

            if (File.Exists("sentry.bin"))
            {
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");

                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username =  user,
                Password =  pass,

                AuthCode = authCode,

                SentryFileHash = sentryHash,
            });
        }

        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result == EResult.AccountLogonDenied)
            {
                Console.WriteLine("Bu Hesap SteamGuard Tarafından Korunmaktadır.");

                Console.Write("Lütfen SteamGuard Yetkilendirme Kodunu Girin... {0} ",callback.EmailDomain);
                authCode = Console.ReadLine();

                return;
            }
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Steam'e Bağlanılamadı {0}\n", callback);
                isRunning = false;
                return;
            }
            Console.WriteLine(DateTime.Now+"  {0} Başarıyla Giriş Yaptı!", user);
           
        }

        static void UpdateMachineAuthCallback(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Sentry Dosyası Güncelleniyor ...");
            byte[] sentryHash = CryptoHelper.SHAHash(callback.Data);

            File.WriteAllBytes("sentry.bin",callback.Data);
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,
                FileName = callback.FileName,
                BytesWritten = callback.BytesToWrite,
                FileSize = callback.Data.Length,
                Offset = callback.Offset,
                Result = EResult.OK,
                LastError = 0,
                OneTimePassword = callback.OneTimePassword,
                SentryFileHash = sentryHash,
            });

            Console.WriteLine("Tamamlandı.");
        }

        static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Console.WriteLine("\n {0} Bağlantısı Koptu, 5 Saniye İçinde Yeniden Bağlanılacak...\n",user);

            Thread.Sleep(TimeSpan.FromSeconds(5));

            steamClient.Connect();
        }

        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            steamFriends.SetPersonaState(EPersonaState.Online);

        }

        static void OnChatMessage(SteamFriends.FriendMsgCallback callback)
        {
            string[] args;

            if (callback.EntryType == EChatEntryType.ChatMsg)
            {
                if (callback.Message.Length > 1)
                {
                    if (callback.Message.Remove(1) == "!")
                    {
                        string command = callback.Message;
                        if (callback.Message.Contains(" "))
                        {
                            command = callback.Message.Remove(callback.Message.IndexOf(' '));
                        }

                        switch (command)
                        {
                            case "!send":
                                args = Seperate(2,' ',callback.Message);
                                Console.WriteLine("!send" + args[1] + args[2] + "Komut alındı. Kullanıcı : " + steamFriends.GetFriendPersonaName(callback.Sender));
                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Command Syntax : !send [friend] [message]");
                                    return;
                                }
                                for (int i = 0; i < steamFriends.GetFriendCount(); i++)
                                {
                                    SteamID friend = steamFriends.GetFriendByIndex(i);
                                    if (steamFriends.GetFriendPersonaName(friend).ToLower().Contains(args[1].ToLower()))
                                    {
                                        steamFriends.SendChatMessage(friend, EChatEntryType.ChatMsg, args[2]);
                                    }
                                }
                                break;

                            case "!hi":
                                Console.WriteLine(DateTime.Now + "   Selam Komutu Alındı. Kullanıcı Adı :" + steamFriends.GetFriendPersonaName(callback.Sender));
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Hi");
                                break;

                            case "!selam":
                                Console.WriteLine(DateTime.Now + "   Selam Komutu Alındı. Kullanıcı Adı :" + steamFriends.GetFriendPersonaName(callback.Sender));
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Aleyküm Selam");
                                break;
                            case "!senkimsin":
                                Console.WriteLine(DateTime.Now +"   Kimsin Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Ben bir botum, Adil adına çalışıyorum.");
                                break;
                            case "!whoareyou":
                                Console.WriteLine(DateTime.Now + "   Kimsin Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "I'm a bot. I'm working for Adil.");
                                break;
                            case "!tarih":
                                Console.WriteLine(DateTime.Now + "   Tarih Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, DateTime.Now.ToString());
                                break;
                            case "!date":
                                Console.WriteLine(DateTime.Now + "   Tarih Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, DateTime.Now.ToString());
                                break;
                            case "!help":
                                Console.WriteLine(DateTime.Now + "   Yardım Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!whoareyou for about me. "+ Environment.NewLine + "!date for current date." + Environment.NewLine + "!note for leaving a note" +Environment.NewLine + "!mail for sending mail" + Environment.NewLine + "!key for get key if you win a game" + Environment.NewLine + "!donate for donating game keys");
                                break;
                            case "!yardım":
                                Console.WriteLine(DateTime.Now + "   Yardım Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "!tarih : Tarih bilgisi için." + Environment.NewLine + "!senkimsin : Hakkımda Bilgi İçin" + Environment.NewLine + "!not : Not Bırakmak İçin" +Environment.NewLine + "!email : Mail yollamak için" + Environment.NewLine + "!key : Oyun kazandıysanız almak için." + Environment.NewLine + "!bağış : Oyun keyi hediye etmek için");
                                break;

                            case "!bağış":
                                args = Seperate(1, ' ', callback.Message);

                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Yanlış Kullanım. " + Environment.NewLine + "Örnek Kullanım : !Bağış 11111-11111-11111 Oyun Adı");
                                    return;
                                }

                                Console.WriteLine(DateTime.Now + "   Bir yeni oyun alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                using (StreamWriter writer = new StreamWriter(steamFriends.GetFriendPersonaName(callback.Sender) + "'in Bagisi" + ".txt", true))
                                {
                                    writer.Write(DateTime.Now + "   " + args[1] + Environment.NewLine);
                                }

                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bağış için teşekkürler.");
                                break;

                            case "!donate":
                                args = Seperate(1, ' ', callback.Message);

                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Wrong message. " + Environment.NewLine + "Example Using : !donate 11111-11111-11111 Game Name");
                                    return;
                                }

                                Console.WriteLine(DateTime.Now + "   Bir yeni oyun alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                using (StreamWriter writer = new StreamWriter(steamFriends.GetFriendPersonaName(callback.Sender) + "'in Bagisi" + ".txt", true))
                                {
                                    writer.Write(DateTime.Now + "   " + args[1] + Environment.NewLine);
                                }

                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Thanks for donate.");
                                break;

                            case "!adinidegistir":
                                if (isBotAdmin(callback.Sender))
                                {
                                    args = Seperate(1,' ',callback.Message);

                                    Console.WriteLine(DateTime.Now + "   İsim değiştirme komutu alındı.");

                                    if (args[0] == "-1")
                                    {
                                        steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Yanlış giriş. !adınıdeğiştir [yeni isim]");
                                        return;
                                    }

                                    steamFriends.SetPersonaName(args[1]);
                                }

                                else
                                {
                                    Console.WriteLine(DateTime.Now + "İsim değiştirme komutu alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Siz Yönetici Değilsiniz");
                                }


                                break;

                            case "!keyekle":
                                if (isBotAdmin(callback.Sender))
                                {
                                    args = Seperate(2, ' ', callback.Message);

                                    Console.WriteLine(DateTime.Now + "   "+ args[1] +" Dosyasına Key Eklendi.");

                                    if (args[0] == "-1")
                                    {
                                        steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Yanlış giriş. !keyekle [Alıcı S64] [Keyler]");
                                        return;
                                    }

                                    using (StreamWriter writer = new StreamWriter( args[1] + ".txt", true))
                                    {
                                        writer.Write(args[2] + Environment.NewLine);
                                    }

                                }

                                else
                                {
                                   // Console.WriteLine(DateTime.Now + "İsim değiştirme komutu alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Bu Özelliği Kullanabilmek İçin Yönetici Olmalısınız.");
                                }


                                break;

                            case "!kapan":
                                if (isBotAdmin(callback.Sender))
                                {
                                    Console.WriteLine(DateTime.Now + "Kapatma Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "5 Saniye Sonra Kapanıyorum Efendim");
                                    Thread.Sleep(5000);
                                    steamUser.LogOff();
                                }
                                else
                                {
                                    Console.WriteLine(DateTime.Now + "Kapatma Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Siz Yönetici Değilsiniz");
                                }
                                
                                break;

                            case "!key":

                              /*  if (isBotAdmin(callback.Sender))
                                {
                                    Console.WriteLine(DateTime.Now + " Bir Oyun Keyi Verildi. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                    string ver = "";
                                    using (StreamReader sr = new StreamReader("Keys.txt"))
                                    {
                                        ver = sr.ReadToEnd();
                                    }
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg,"Here your game : " + ver);
                                }*/

                                if (File.Exists(callback.Sender.AccountID.ToString() + ".txt"))
                                {
                                    
                                    string ver = "";
                                    using (StreamReader sr = new StreamReader(callback.Sender.AccountID.ToString() + ".txt"))
                                    {
                                       ver = sr.ReadToEnd();
                                    }
                                    Console.WriteLine(DateTime.Now + " Bir Oyun Keyi Verildi. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, ver);
                                }

                                else
                                {
                                    Console.WriteLine(DateTime.Now + " Geçersiz Oyun Keyi İsteği. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender) + "Steam Id : " + callback.Sender.AccountID);
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Maalesef herhangi bir oyun kazanmamışsınız :(" + Environment.NewLine + "Sorry you didn't win any game :(");
                                }
                               
                                break;

                            case "!note":

                                args = Seperate(1, ' ', callback.Message);

                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Wrong message. " + Environment.NewLine + "Example Using : !note Heyyy. Howdy ?");
                                    return;
                                }

                                Console.WriteLine(DateTime.Now + "  Bir yeni not. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                using (StreamWriter writer = new StreamWriter(steamFriends.GetFriendPersonaName(callback.Sender) + "'in Notu" + ".txt", true))
                                {
                                    writer.Write( DateTime.Now + "   " + args[1] + Environment.NewLine );
                                }

                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Your note succesfully saved.");
                                break;

                            case "!not":

                                args = Seperate(1, ' ', callback.Message);

                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Yanlış Mesaj. " + Environment.NewLine + "Örnek Kullanım : !note Selam. Nasılsın ?");
                                    return;
                                }

                                Console.WriteLine(DateTime.Now + "  Bir yeni not. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                using (StreamWriter writer = new StreamWriter(steamFriends.GetFriendPersonaName(callback.Sender) + "'in Notu" + ".txt", true))
                                {
                                    writer.Write(DateTime.Now + "   " + args[1] + Environment.NewLine);
                                }

                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Notunuz Başarıyla Kaydedildi.");
                                break;

                            case "!mail":
                                

                                args = Seperate(1, ' ', callback.Message);

                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Wrong message ! " + Environment.NewLine + "Example Using : !mail Heyyy. Howdy ?");
                                }

                                else
                                {
                                    MailAt(callback.Sender , args[1],false);
                                }

                                break;

                            case "!email":


                                args = Seperate(1, ' ', callback.Message);

                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Yanlış sözdizimi ! " + Environment.NewLine + "Örnek Kullanım : !mail Nasılsın ?");
                                }

                                else
                                {
                                    MailAt(callback.Sender, args[1],true);

                                }

                                break;

                            /*   case "!Komut":
                                    Console.WriteLine(DateTime.Now + "Kimsin Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                    steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Ben bir botum, Adil adına çalışıyorum.");
                                    break; */

                            default:
                                steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Dediğinizi anlamadım komut listesi için !yardım . " + Environment.NewLine + "I don't understand. Use !help command if you want help.");
                                Console.WriteLine(DateTime.Now + "  Bilinmeyen Bir Komut Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                break;
                        }

                        

                    }

                    else
                    {
                        steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Dediğinizi anlamadım komut listesi için !yardım . " + Environment.NewLine + "I don't understand. Use !help command if you want help.");
                        Console.WriteLine(DateTime.Now + "  Bilinmeyen Bir Komut Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                    }
                }
            }

        }

        static void OnFriendLists(SteamFriends.FriendsListCallback callback)
        {
            Thread.Sleep(2500);
            foreach (var friend in callback.FriendList )
            {
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    steamFriends.AddFriend(friend.SteamID);
                    Thread.Sleep(500);
                    steamFriends.SendChatMessage(friend.SteamID, EChatEntryType.ChatMsg, "Ben Bir Botum. \nI'm a bot.");
                }
            }
        }

        public static bool isBotAdmin(SteamID sid)
        {
            try
            {
                if (sid.ConvertToUInt64() == Convert.ToUInt64(File.ReadAllText("admin.txt")))
                {
                    return true;
                }

              //  steamFriends.SendChatMessage(sid, EChatEntryType.ChatMsg, "Siz Botun Yöneticisi Değilsiniz");
                Console.WriteLine(DateTime.Now + "  " + steamFriends.GetFriendPersonaName(sid) + " adlı kullanıcı yönetici komutu kullandı !");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        static void OnChatInvite(SteamFriends.ChatInviteCallback callback)
        {
            steamFriends.JoinChat(callback.ChatRoomID);
            Console.WriteLine(steamFriends.GetFriendPersonaName(callback.PatronID) + " Adlı Kullanıcı, Botu " + callback.ChatRoomName + " Adlı Sohbet Odasına Davet Etti." + " (" + callback.ChatRoomID.ConvertToUInt64().ToString() + ")");
        }

        static void OnChatEnter(SteamFriends.ChatEnterCallback callback)
        {
            steamFriends.SendChatRoomMessage(callback.ChatID, EChatEntryType.ChatMsg, "Merhaba Ben Bir Botum. \nHi I'm a bot.");
        }

        static void OnGrupMsg(SteamFriends.ChatMsgCallback callback)
        {
            string[] args;

            if (callback.ChatMsgType == EChatEntryType.ChatMsg)
            {
                if (callback.Message.Length > 1)
                {
                    if (callback.Message.Remove(1) == "!")
                    {
                        string command = callback.Message;
                        if (callback.Message.Contains(" "))
                        {
                            callback.Message.Remove(callback.Message.IndexOf(" "));
                        }

                        switch (command)
                        {
                            case "!send":
                                args = Seperate(2, ' ', callback.Message);
                                Console.WriteLine("!send" + args[1] + args[2] + "Komut alındı. Kullanıcı :" + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "Command Syntax : !send [friend] [message]");
                                    return;
                                }
                                for (int i = 0; i < steamFriends.GetFriendCount(); i++)
                                {
                                    SteamID friend = steamFriends.GetFriendByIndex(i);
                                    if (steamFriends.GetFriendPersonaName(friend).ToLower().Contains(args[1].ToLower()))
                                    {
                                        steamFriends.SendChatMessage(friend, EChatEntryType.ChatMsg, args[2]);
                                    }
                                }
                                break;

                            case "!hi":
                                Console.WriteLine(DateTime.Now + "   Selam Komutu Alındı. Kullanıcı Adı :" + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "Hi");
                                break;

                            case "!selam":
                                Console.WriteLine(DateTime.Now + "   Selam Komutu Alındı. Kullanıcı Adı :" + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "Aleyküm Selam");
                                break;
                            case "!senkimsin":
                                Console.WriteLine(DateTime.Now + "   Kimsin Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "Ben bir botum, Adil adına çalışıyorum.");
                                break;
                            case "!whoareyou":
                                Console.WriteLine(DateTime.Now + "   Kimsin Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "I'm a bot. I'm working for Adil.");
                                break;
                            case "!tarih":
                                Console.WriteLine(DateTime.Now + "   Saat Kaç Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, DateTime.Now.ToString());
                                break;
                            case "!date":
                                Console.WriteLine(DateTime.Now + "   Tarih Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, DateTime.Now.ToString());
                                break;
                            case "!help":
                                Console.WriteLine(DateTime.Now + "   Yardım Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "!hi,!selam,!senkimsin,!whoareyou,!tarih,!date \nYeni Özelliklerim Eklenecek :)");
                                break;
                            case "!yardım":
                                Console.WriteLine(DateTime.Now + "   Yardım Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "!hi,!selam,!senkimsin,!whoareyou,!tarih,!date \nYeni Özelliklerim Eklenecek :)");
                                break;
                            case "!adınıdeğiştir":
                                if (!isBotAdmin(callback.ChatterID))
                                    return;
                                args = Seperate(1, ' ', callback.Message);
                                Console.WriteLine(DateTime.Now + "   İsim değiştirme komutu alındı.");
                                if (args[0] == "-1")
                                {
                                    steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "Yanlış giriş. !adınıdeğiştir [yeni isim]");
                                    return;
                                }

                                steamFriends.SetPersonaName(args[1]);
                                break;

                            case "!kapan":
                                if (isBotAdmin(callback.ChatterID))
                                {
                                    Console.WriteLine(DateTime.Now + "Kapatma Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                    steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "5 Saniye Sonra Kapanıyorum Efendim");
                                    Thread.Sleep(5000);
                                    steamUser.LogOff();
                                }
                                else
                                {
                                    Console.WriteLine(DateTime.Now + "Kapatma Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                                    steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "Siz Yönetici Değilsiniz");
                                }

                                break;
                                /*     case "!envanter":
                                         Console.WriteLine(DateTime.Now + "Kimsin Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                         steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Ben bir botum, Adil adına çalışıyorum.");
                                         break;
                                    case "!senkimsin":
                                         Console.WriteLine(DateTime.Now + "Kimsin Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                         steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Ben bir botum, Adil adına çalışıyorum.");
                                         break;
                                     case "!senkimsin":
                                         Console.WriteLine(DateTime.Now + "Kimsin Komutu Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.Sender));
                                         steamFriends.SendChatMessage(callback.Sender, EChatEntryType.ChatMsg, "Ben bir botum, Adil adına çalışıyorum.");
                                         break; */

                        }



                    }

                    else
                    {
                        steamFriends.SendChatMessage(callback.ChatterID, EChatEntryType.ChatMsg, "Dediğinizi anlamadım komut listesi için !help");
                        Console.WriteLine("Bilinmeyen Bir Komut Alındı. Kullanıcı Adı: " + steamFriends.GetFriendPersonaName(callback.ChatterID));
                    }
                }
            }
        }

        public static string[] Seperate(int number, char seperator, string thestring)
        {
            string[] returned = new string[4];

            int i = 0;

            int error = 0;

            int length = thestring.Length;

            foreach (char c in thestring) //!friend
            {
                if (i != number)
                {
                    if (error > length || number > 5)
                    {
                        returned[0] = "-1";
                        return returned;
                    }
                    else if (c == seperator)
                    {
                        //returned[0] = !friend
                        returned[i] = thestring.Remove(thestring.IndexOf(c));
                        thestring = thestring.Remove(0, thestring.IndexOf(c) + 1);
                        i++;
                    }
                    error++;

                    if (error == length && i != number)
                    {
                        returned[0] = "-1";
                        return returned;
                    }
                }
                else
                {
                    returned[i] = thestring;
                }
            }
            return returned;
        }

        static void MailAt(SteamID gonderen , string icerik , bool turkce)
        {
            try
            {
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.From = new MailAddress(mailId);
                mail.To.Add(receiverMail);
                mail.IsBodyHtml = true;
                mail.Subject = "Kullanıcı Maili";
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.Body = steamFriends.GetFriendPersonaName(gonderen) + " Adlı Kulllanıcının Maili : " + icerik;
                SmtpClient sc = new SmtpClient();
                sc.Host = "smtp.yandex.com.tr";
                sc.Port = 587;
                sc.EnableSsl = true;
                sc.UseDefaultCredentials = false;
                sc.Credentials = new NetworkCredential(mailId, mailPass);
                sc.Send(mail);
               
                Console.WriteLine(DateTime.Now + "  Mail Alındı. Kullanıcı Adı : " + steamFriends.GetFriendPersonaName(gonderen));
                if (turkce == false)
                {
                  steamFriends.SendChatMessage(gonderen, EChatEntryType.ChatMsg, "Successfully Sended !");
                }
                else
                {
                    steamFriends.SendChatMessage(gonderen, EChatEntryType.ChatMsg, "Başarıyla Gönderildi !");
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Hata" + ex.ToString());
                if (turkce == false)
                {
                    steamFriends.SendChatMessage(gonderen, EChatEntryType.ChatMsg, "Sending Failed !");
                }
                else
                {
                    steamFriends.SendChatMessage(gonderen, EChatEntryType.ChatMsg, "Mail Gönderilemedi !");
                }
            }
        }
    }
}
