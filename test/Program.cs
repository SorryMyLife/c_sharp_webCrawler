/*
 * 首先，这个工具类是从Java上移植过来的
 其次这个工具类其实并没有完善，充满了bug，比如每个类的正则需要再进行一次修改
 然后就是那个Dictionary建议全部换成linkedlist
 目前就只在酷狗里面完成了替换，其他几个还没有替换
 其实这个早就完成了，只是我比较懒，没有进行测试
 然后前几天测试的时候，发现这个命令行参数不行，会额外读取一行回车
 就不想搞了，我还是继续用Java版的吧
 Java版本的还是比较好用的
 
 build_time : 2018年10月11日20:09:42
 
 .net_framwork_version : 4.x+
 
 build_version : build-2
 
 */


//需要using的一些基本姿势，虽然这些不是必要的，但可以减少一定的代码量，虽然并没有什么卵用
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.IO;

class Kv //用来存储键值对的，临时搭建的
{
    private object key = "", value = "";
    public void setKey(object key)
    {
        this.key = key;
    }

    public void setValue(object value)
    {
        this.value = value;
    }

    public object getKey()
    {
        return key;
    }

    public object getValue()
    {
        return value;
    }

    public void setAll(object key , object value)
    {
        this.key = key;
        this.value = value;
    }

}

class t
{
    //这个是一个临时搭建的工具类，还是多多少少有些不完善，但勉强可以用
    public string code = "", tmp = "" , parm = "Content-Encoding=gzip"  , line = "";
    public int count = 1 , nn = 0 ,ee =0 , size = 0 ,bc = 1;
    public string ss = Convert.ToString(Console.ReadLine());
    public Kv k;
    public WebClient checkCon() //配置请求头参数
    {
        WebClient wc = new WebClient();
       
        wc.Headers.Add("Content-Type","text/plain");
        wc.Headers.Add("Charset", "UTF-8");
        wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.92 Safari/537.36");
        wc.Headers.Add("parm",parm);


        return wc;
    }

    public String printURL(string url_name) //打印网页内容
    {
        byte[] buff = checkCon().DownloadData(url_name);
        code = Encoding.GetEncoding("utf-8").GetString(buff);

        return code;
    }

    public void down(string url_name , string save_path , string save_name) //下载函数，用来下载歌曲用的，也可以用来下载别的内容，比如二进制文件、文本文件等等
    {
        tmp = save_path + "\\" + save_name;
        if (save_name.IndexOf("\"")!=-1)
        {
            save_name = save_name.Replace("\"","-");
        }
        if(!System.IO.Directory.Exists(save_path))
        {
            System.IO.Directory.CreateDirectory(save_path);
        }
        else
        {
            if(!System.IO.File.Exists(tmp))
            {
                System.Console.WriteLine("正在开始下载: "+save_name);
                checkCon().DownloadFile(url_name,tmp);
                System.Console.WriteLine("下载完成: " + save_name);

            }
            else
            {
                Console.WriteLine(save_name+" 已经存在");
                down(url_name,save_path,save_name+bc);
                bc++;
            }
        }
    }

    public ArrayList getCust(String url_name, String reg, String re_str) //这是一个自定义的正则捕获
    {
        ArrayList list = new ArrayList();
        code = printURL(url_name);
        Regex r = new Regex(reg);
        Match m = r.Match(code);
        while (m.Success)
        {
            //Console.WriteLine(m.Value.Replace(re_str, ""));
            tmp = m.Value.Replace(re_str, "");
            list.Add(tmp);
            m = m.NextMatch();
        }
        return list;
    }

}

class cool_dog : t //这个是操作酷狗音乐的类，里面有酷狗音乐的api，你可以用于其他的语言进行操作
{
    private String index_url = "http://wwwapi.kugou.com/yy/index.php?r=play/getdata&callback=jQuery1910027104170248473114_1535258754450&hash="; //将歌曲的hash值放在这里就可以获取歌曲的真实链接
    private string m_name = "", m_hash = "";

    public void init()
    {
        ss = null;
        checkCon().Headers.Add("Cookie", "kg_mid=f1258f0976aacd00c41230cc38c3a96b; Hm_lvt_aedee6983d4cfc62f509129360d6bb3d=1531743036; Hm_lpvt_aedee6983d4cfc62f509129360d6bb3d=1531744752");
    }

    public static string StringToUnicode(string s) //字符串转Unicode函数
    {
        char[] charbuffers = s.ToCharArray();
        byte[] buffer; StringBuilder sb = new StringBuilder();
        for (int i = 0; i < charbuffers.Length; i++)
        {
            buffer = System.Text.Encoding.Unicode.GetBytes(charbuffers[i].ToString());
            sb.Append(String.Format("\\u{0:X2}{1:X2}", buffer[1], buffer[0]));
        }
        return sb.ToString();
    } 

    public static string UnicodeToString(string srcText) //Unicode转字符串函数
    {
        string dst = "";
        string src = srcText;
        int len = srcText.Length / 6;
        for (int i = 0; i <= len - 1; i++)
        {
            string str = "";
            str = src.Substring(0, 6).Substring(2);
            src = src.Substring(6);
            byte[] bytes = new byte[2];
            bytes[1] = byte.Parse(int.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber).ToString());
            bytes[0] = byte.Parse(int.Parse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber).ToString());
            dst += Encoding.Unicode.GetString(bytes);
        }
        return dst;
    }

    public void getMusicList(string url_name , LinkedList<Kv> list) //这个是用来获取歌单的，我相信你应该看得懂这个英文
    {
        String reg = "<li>(.+?\\</li>)";
        String hashReg = "(.+?\\<input)";
        String music_reg = "\\S*\"";
        String hash_reg = "\"\\S*";
        code = printURL(url_name);
        Match m1 = new Regex(reg).Match(code);
        while(m1.Success)
        {
            if(m1.Value.IndexOf("<a title") != -1)
            {
                Match m2 = new Regex(hashReg).Match(m1.Value);
                while(m2.Success)
                {
                    tmp = m2.Value.Replace("hidefocus=\"true\" href=\"javascript:;\" data=\"|\"><input|\\|\\d*|<li><a title=\"| ", "");
                    Match m3 = new Regex(music_reg).Match(tmp);
                    Match m4 = new Regex(hash_reg).Match(tmp);
                    while(m3.Success|m4.Success)
                    {
                        m_name = m3.Value.Replace("\"","");
                        m_hash = m4.Value.Replace("\"","");
                        //map.Add(m_hash,m_name);
                        k = new Kv();
                        k.setAll(m_hash, m_name);
                        list.AddLast(k);
                        m3 = m3.NextMatch();
                        m4 = m4.NextMatch();
                    }
                    m2 = m2.NextMatch();
                }
            }
            m1 = m1.NextMatch();
        }

    }

    public String getMusic(string url_name) //这个是用来获取音乐链接的
    {
        code = printURL(url_name);
        String str1 = "", musicUrl = "";
        String all = "\"play_url\"\\S*.mp3";

        Match m1 = new Regex(all).Match(code);
        while(m1.Success)
        {
            str1 = m1.Value.Replace("play_url|\"|:|\\\\|http|https", "");
            musicUrl = "http:" + UnicodeToString(str1);
            m1 = m1.NextMatch();
        }
        return musicUrl;
    }

    public void searchMusic(String music_name , LinkedList<Kv> list) //这个是用来搜索音乐的
    {
        tmp = music_name;
        music_name = HttpUtility.UrlEncode(tmp,System.Text.Encoding.UTF8);
        String reg = "\"FileHash\":\"(.+?\\S*\",\"SQPa)";
        String n_reg = "\"FileName\":\"(.+?\\S*)\",\"Oth";
        for(int i = 1;i<31;i++)
        {
            String search_music = "http://songsearch.kugou.com/song_search_v2?callback=jQuery112405213552049562944_1505739248953&keyword=" + music_name + "&page=" + i + "&pagesize=30&userid=-1&clientver=&platform=WebFilter";
            code = printURL(search_music);
            Match m1 = new Regex(reg).Match(code);
            Match m2 = new Regex(n_reg).Match(code);
            while(m1.Success|m2.Success)
            {
                m_hash = m1.Value.Replace("FileHash\":\"|\",\"SQPa|\"", "");
                m_name = m2.Value.Replace("\"FileName\":\"|\",\"Oth| ", "");
                //map.Add(m_hash,m_name);
                k = new Kv();
                k.setAll(m_hash,m_name);
                list.AddLast(k);
                m1 = m1.NextMatch();
                m2 = m2.NextMatch();
            }
        }

    }

    public void getInfo(string url_name , LinkedList<Kv> list) //这个是获取歌手所有歌曲信息的
    {
        String All = "value=\"(.+?\\S*\" />)";
        String name = "value=\"(.+?\\D*\\|)";
        String hash = "\\|(.+?\\D*\\|)";
        code = printURL(url_name);
        Match m1 = new Regex(All).Match(code);
        while(m1.Success)
        {
            line = m1.Value.Replace("\" />| ","");
            Match m2 = new Regex(name).Match(line);
            Match m3 = new Regex(hash).Match(line);
            while (m2.Success|m3.Success)
            {
                //map.Add(m2.Value.Replace("\\|",""),m3.Value.Replace("value=\"|\\|", ""));
                k = new Kv();
                k.setAll(m2.Value.Replace("\\|", ""), m3.Value.Replace("value=\"|\\|", ""));
                list.AddLast(k);
                m2 = m2.NextMatch();
                m3 = m3.NextMatch();
            }

            m1 = m1.NextMatch();
        }


    }

    public void start_list(string save_path , string url_name) //开始获取歌单操作
    {
        object hash = " ", music_name = " ", music_url = " ";
        LinkedList<Kv> list = new LinkedList<Kv>();
        getMusicList(url_name,list);
        size = list.Count;
        System.Console.WriteLine("一共有: " + size + " 首歌曲!");
        foreach(Kv e in list)
        {
            music_name = e.getValue();
            hash = e.getKey();
            tmp = index_url + hash;
            music_url = getMusic(tmp);
            System.Console.WriteLine(count + " - " + music_name + " 开始下载 ");
            down((string)music_url,save_path,music_name+".mp3");
            System.Console.WriteLine(count + " - " + music_name + " 下载完成 !");
            count++;
        }
        Console.WriteLine("一共下载了 " + (count - 1) + " 首歌曲");
        Console.WriteLine("下载失败: " + ((size - count) + 1) + " 首歌曲");

    }

    public void start(string save_path, string url_name) //开始获取歌手所有歌曲的操作
    {
        object hash = " ", music_name = " ", music_url = " ";
        LinkedList<Kv> list = new LinkedList<Kv>();
        getInfo(url_name, list);
        size = list.Count;
        System.Console.WriteLine("一共有: " + size + " 首歌曲!");
        foreach(Kv v in list)
        {
            music_name = v.getValue();
            hash = v.getKey();
            music_url = getMusic(index_url + hash + "&album_id=0&_=1531744751601");
            System.Console.WriteLine(count + " - " + music_name + " 开始下载 ");
            down((string)music_url, save_path, music_name + ".mp3");
            System.Console.WriteLine(count + " - " + music_name + " 下载完成 !");
            count++;
        }

    }

    public void start_list_sel(string save_path, string url_name)
    {
        object hash = " ", music_name = " ", music_url = " ";
        LinkedList<Kv> list = new LinkedList<Kv>();
        Dictionary<int, Kv> tmp_map = new Dictionary<int, Kv>();
        getMusicList(url_name, list);
        nn = 1;
        size = list.Count;
        System.Console.WriteLine("一共有: " + size + " 首歌曲!");
        foreach(Kv e in list)
        {
            tmp_map.Add(nn, e);
            nn++;
        }
        nn = 0;
        foreach(KeyValuePair<int,Kv> e in tmp_map)
        {
            Console.Write(e.Key + " -- "+e.Value.getValue() + "\t");
            if(nn == 10)
            {
                Console.WriteLine();
                nn = 0;
            }
            nn++;
        }
        if(size > 1)
        {
            Console.WriteLine("请输入需要下载的歌曲数目: ");
            int numm = int.Parse(ss);
            for(int i = 0;i<numm;i++)
            {
                System.Console.WriteLine("\n\n=======>请选择需要进行下载的歌曲序号(1-9): ");
                int ii = int.Parse(ss);
                foreach (KeyValuePair<int, Kv> e in tmp_map)
                {
                    if(ii == e.Key)
                    {
                        tmp = getMusic(index_url+e.Value.getKey());
                        m_name = (string)e.Value.getValue();
                    }
                }
                if(tmp.Length >1)
                {
                    Console.WriteLine(count + " - " + m_name + " 开始下载");
                    down(tmp,save_path,m_name+".mp3");
                    Console.WriteLine(count + " - " + m_name + " 下载完成\n文件保存在: " + save_path);
                }
                else
                {
                    Console.WriteLine("选择的这首歌曲存在不知名的问题,暂时没有解决");
                    return;
                }
            }

        }
        else
        {
            System.Console.WriteLine("请选择一首歌曲");
        }
        System.Console.WriteLine();

    }

    public void start_search_sel(string search_name , string save_path)
    {
        object hash = " ", music_name = " ", music_url = " ";
        LinkedList<Kv> list = new LinkedList<Kv>();
        Dictionary<int, Kv> tmp_map = new Dictionary<int, Kv>();
        searchMusic(search_name,list);
        nn = 1;
        size = list.Count;
        System.Console.WriteLine("一共有: " + size + " 首歌曲!");
        foreach (Kv e in list)
        {
            tmp_map.Add(nn, e);
            nn++;
        }
        nn = 0;
        foreach (KeyValuePair<int ,Kv> e in tmp_map)
        {
            Console.Write(e.Key + " -- " + e.Value.getValue() + "\t");
            if (nn == 10)
            {
                Console.WriteLine();
                nn = 0;
            }
            nn++;
        }
        if (size > 1)
        {
            Console.WriteLine("请输入需要下载的歌曲数目: ");
            int numm = Convert.ToInt32(ss);
            for (int i = 0; i < numm; i++)
            {
                System.Console.WriteLine("\n\n=======>请选择需要进行下载的歌曲序号(1-9): ");
                int ii = int.Parse(ss);
                foreach (KeyValuePair<int, Kv> e in tmp_map)
                {
                    if (ii == e.Key)
                    {
                        tmp = getMusic(index_url + e.Value.getKey());
                        m_name = (string)e.Value.getValue();
                    }
                }
                if (tmp.Length > 1)
                {
                    Console.WriteLine(count + " - " + m_name + " 开始下载");
                    down(tmp, save_path, m_name + ".mp3");
                    Console.WriteLine(count + " - " + m_name + " 下载完成\n文件保存在: " + save_path);
                }
                else
                {
                    Console.WriteLine("选择的这首歌曲存在不知名的问题,暂时没有解决");
                    return;
                }
            }

        }
        else
        {
            System.Console.WriteLine("请选择一首歌曲");
        }
        System.Console.WriteLine();

    }

    public void start_search(string search_name, string save_path) //开始获取搜索歌曲的所有内容
    {
        object hash = " ", music_name = " ", music_url = " ";
        LinkedList<Kv> list = new LinkedList<Kv>();
        searchMusic(search_name, list);
        size = list.Count;
        System.Console.WriteLine("一共有: " + size + " 首歌曲!");
        foreach (Kv e in list)
        {
            music_name = e.getValue();
            hash = e.getKey();
            tmp = index_url + hash;
            music_url = getMusic(tmp);
            System.Console.WriteLine(count + " - " + music_name + " 开始下载 ");
            down((string)music_url, save_path, music_name + ".mp3");
            System.Console.WriteLine(count + " - " + music_name + " 下载完成 !");
            count++;
        }
        Console.WriteLine("一共下载了 " + (count - 1) + " 首歌曲");
        Console.WriteLine("下载失败: " + ((size - count) + 1) + " 首歌曲");

    }

    public void start_start_sel(string save_path, string url_name)
    {
        object hash = " ", music_name = " ", music_url = " ";
        LinkedList<Kv> list = new LinkedList<Kv>();
        Dictionary<int, Kv> tmp_map = new Dictionary<int, Kv>();
        getInfo(url_name,list);
        nn = 1;
        size = list.Count;
        System.Console.WriteLine("一共有: " + size + " 首歌曲!");
        foreach (Kv e in list)
        {
            tmp_map.Add(nn, e);
            nn++;
        }
        nn = 0;
        foreach (KeyValuePair<int, Kv> e in tmp_map)
        {
            Console.Write(e.Key + " -- " + e.Value.getValue() + "\t");
            if (nn == 10)
            {
                Console.WriteLine();
                nn = 0;
            }
            nn++;
        }
        if (size > 1)
        {
            Console.WriteLine("请输入需要下载的歌曲数目: ");
            int numm = int.Parse(ss);
            for (int i = 0; i < numm; i++)
            {
                System.Console.WriteLine("\n\n=======>请选择需要进行下载的歌曲序号(1-9): ");
                int ii = int.Parse(ss);
                foreach (KeyValuePair<int, Kv> e in tmp_map)
                {
                    if (ii == e.Key)
                    {
                        tmp = getMusic(index_url + e.Value.getKey());
                        m_name = (string)e.Value.getValue();
                    }
                }
                if (tmp.Length > 1)
                {
                    Console.WriteLine(count + " - " + m_name + " 开始下载");
                    down(tmp, save_path, m_name + ".mp3");
                    Console.WriteLine(count + " - " + m_name + " 下载完成\n文件保存在: " + save_path);
                }
                else
                {
                    Console.WriteLine("选择的这首歌曲存在不知名的问题,暂时没有解决");
                    return;
                }
            }

        }
        else
        {
            System.Console.WriteLine("请选择一首歌曲");
        }
        System.Console.WriteLine();

    }


}

class music_163 : t //这个是操作网易云音乐的类，里面有网易云音乐的api，你可以用于其他的语言进行操作
{
    private string index_id = "http://music.163.com/song/media/outer/url?id="; //把歌曲的id贴在这里，就可以获取真实内容
    private string music_id = " ", music_name = " ";

    public void init()
    {
        ss = null;
        checkCon().Headers.Add("Accept", "text/html");
        checkCon().Headers.Add("Accept-Encoding", "br");
        checkCon().Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
    }

    public void getInfo(string url_name , Dictionary<string , string> map) //获取歌手或者歌单的所有歌曲信息
    {
        if(url_name.IndexOf("#")!=-1)
        {
            url_name = url_name.Replace("#/","");
        }
        code = printURL(url_name);
        String reg = "<li><a href=\"/song(.+?\\S*</a>)";
        String id_reg = "id=\\S*\">";
        String name_reg = "\">(.+?\\S*</a)";
        String id = "", name = "";
        Match m1 = new Regex(reg).Match(code);
        while(m1.Success)
        {
            tmp = m1.Value;
            Match m2 = new Regex(id_reg).Match(tmp);
            Match m3 = new Regex(name_reg).Match(tmp);
            while(m2.Success|m3.Success)
            {
                id = m2.Value.Replace("id=|\">", "");
                name = m3.Value.Replace("\">|</a|", "").Replace(" ", "-");
                map.Add(id,name);
                m2 = m2.NextMatch();
                m3 = m3.NextMatch();
            }
            m1 = m1.NextMatch();
        }


    }

    public void start(string url_name , string save_path) //开始函数
    {
        Dictionary<string, string> map = new Dictionary<string, string>();
        getInfo(url_name,map);
        size = map.Count;
        System.Console.WriteLine("一共找到: " + size + " 首歌曲!");
        foreach(KeyValuePair<string,string> e in map)
        {
            music_id = e.Key;
            music_name = e.Value;
            tmp = index_id + music_id;
            System.Console.WriteLine(count + " - " + music_name + " 开始下载");
            down(tmp,save_path,music_name+".mp3");
            System.Console.WriteLine(count + " - " + music_name + " 下载完成");
            count++;
        }
        System.Console.WriteLine("一共下载了 " + (count - 1) + " 首歌曲");
        System.Console.WriteLine("下载失败: " + ((size - count) + 1) + " 首歌曲");



    }

    public void start_sel(string url_name, string save_path)
    {
        Dictionary<string, string> map = new Dictionary<string, string>();
        Dictionary<int, KeyValuePair<string, string>> tmp_map = new Dictionary<int, KeyValuePair<string, string>>();
        nn = 1;
        getInfo(url_name, map);
        size = map.Count;
        System.Console.WriteLine("一共找到: " + size + " 首歌曲!");
        foreach (KeyValuePair<string, string> e in map)
        {
            tmp_map.Add(nn, e);
            nn++;
        }
        nn = 0;
        foreach(KeyValuePair<int , KeyValuePair<string,string>> e in tmp_map)
        {
            Console.WriteLine(e.Key+" -- "+e.Value.Value+"\t");
            if(nn == 10)
            {
                Console.WriteLine();
                nn = 0;
            }
            nn++;
        }
        if(size >1)
        {
            Console.WriteLine("请输入需要下载的歌曲数目: ");
            int numm = int.Parse(ss);
            if(numm > 1)
            {
                for(int i = 0;i<numm;i++)
                {
                    System.Console.WriteLine("\n\n=======>请选择需要进行下载的歌曲序号(1-9): ");
                    int ii = int.Parse(ss);
                    foreach(KeyValuePair<int,KeyValuePair<string,string>> e in tmp_map)
                    {
                        if(ii == e.Key)
                        {
                            tmp = index_id + e.Value.Key;
                            music_name = e.Value.Value;
                        }
                    }
                    if(tmp.Length>1)
                    {
                        System.Console.WriteLine(count + " - " + music_name + " 开始下载");
                        down(tmp, save_path, music_name + ".mp3");
                        System.Console.WriteLine(count + " - " + music_name + " 下载完成\n文件保存在: " + save_path);
                        count++;
                    }
                    else
                    {
                        System.Console.WriteLine("选择的这首歌曲存在不知名的问题,暂时没有解决");
                    }
                }
            }
            else
            {
                System.Console.WriteLine("请选择一首歌曲");
            }
        }
        else
        {
            Console.WriteLine();
        }

    }



}

class xiami_music : t  //这个是操作虾米音乐的类，里面有虾米音乐的api，你可以用于其他的语言进行操作
{
    //index_url:这个是获取虾米音乐歌曲loaction值用的
    //song_url:这个是用来获取歌曲界面的
    private String index_url = "http://www.xiami.com/widget/xml-single/uid/0/sid/", song_url = "http://www.xiami.com/song/", xiami = "https://www.xiami.com";
    private String music_id = "", code = "", music_name = "", music_url = "", tmp = "", max = " ";
    private int count = 0, num = 1;
    
    public void  init()
    {
        ss = null;
        checkCon().Headers.Add("Cookie", "gid=153837266380742; _xiamitoken=c4f5aedf9d1a8337e6850f24ec59454c; _unsign_token=4a3a4e4b37022b540c0a551704faf652; UM_distinctid=1662e28ccd1d83-01bc868cb0ba4c-8383268-1fa400-1662e28ccd4448; cna=5mImFP8KB2UCAW9PsiIg+QQF; XMPLAYER_volumeValue=1; _uab_collina=153839528743896720191213; xmgid=c8e3606b-6fc5-4f6b-9a50-0d97ac7d9edf; user_from=1; _umdata=2FB0BDB3C12E491D35D2438CFD81B79100066464311F7D13FA1F37072A20020FCAF1D45EAB2C6CBCCD43AD3E795C914C36B27FB0B4D5CA8CF90E64C9464BEFB9; PHPSESSID=05d89faec8a49f74211ffea48419f9ad; CNZZDATA921634=cnzz_eid%3D642129217-1538371972-https%253A%252F%252Fwww.baidu.com%252F%26ntime%3D1538823191; CNZZDATA2629111=cnzz_eid%3D774209260-1538370524-https%253A%252F%252Fwww.baidu.com%252F%26ntime%3D1538824176; isg=BHd3GsjQ4Paza2RzDTuUZFuCBmsBlEv0yzpgyMkkK8ateJe60Q857TxKXJiDiyMW");
    }

    public String getLoaction(String id) //获取loaction值
    {
        code = printURL(index_url + id);
        Match m1 = new Regex("CDATA\\[(.+?\\S*)</loc").Match(code);
        while(m1.Success)
        {
            tmp = m1.Value.Replace("CDATA|></loc", "").Replace("\\[|\\]\\]", "");
            m1 = m1.NextMatch();
        }
        return tmp;
    }

    public String UnLockLoactionToString(String str) //解密loaction值，获取歌曲的真正链接地址，因为这个地址会随时变动，所以需要不断更新
    {
        int _local10;
        int _local2 = int.Parse(str.Substring(0, 1));
        String _local3 = str.Substring(1, str.Length);
        double _local4 = Math.Floor((double)_local3.Length / _local2);
        int _local5 = _local3.Length % _local2;
        String[] _local6 = new String[_local2];
        int _local7 = 0;
        while (_local7 < _local5)
        {
            if (_local6[_local7] == null)
            {
                _local6[_local7] = "";
            }
            _local6[_local7] = _local3.Substring((((int)_local4 + 1) * _local7), (((int)_local4 + 1) * _local7) + ((int)_local4 + 1));
            _local7++;
        }
        _local7 = _local5;
        while (_local7 < _local2)
        {
            _local6[_local7] = _local3.Substring((((int)_local4 * (_local7 - _local5)) + (((int)_local4 + 1) * _local5)), (((int)_local4 * (_local7 - _local5)) + (((int)_local4 + 1) * _local5)) + (int)_local4);
            _local7++;
        }
        String _local8 = "";
        _local7 = 0;
        while (_local7 < ((String)_local6[0]).Length)
        {
            _local10 = 0;
            while (_local10 < _local6.Length)
            {
                if (_local7 >= _local6[_local10].Length) { break; }
                _local8 = (_local8 + _local6[_local10].ToCharArray()[_local7]);
                _local10++;
            }
            _local7++;
        }
            _local8 = HttpUtility.UrlDecode(_local8,System.Text.Encoding.UTF8);
       
        String _local9 = "";
        _local7 = 0;
        while (_local7 < _local8.Length)
        {
            if (_local8.ToCharArray()[_local7] == '^')
            {
                _local9 = (_local9 + "0");
            }
            else
            {
                _local9 = (_local9 + _local8.ToCharArray()[_local7]);
            }; _local7++;
        }
        _local9 = _local9.Replace("+", " ");
        //		System.Console.WriteLine(_local9);
        return _local9;
    }

    public void getInfo(string url_name , Dictionary<string , string> map) //获取歌曲的信息
    {
        code = printURL(url_name);
        Match m1 = new Regex(song_url+"\\d*").Match(code);
        Match m2 = new Regex("的歌曲(.+?\\S*)" + song_url).Match(code);
        while(m1.Success|m2.Success)
        {
            music_id = m1.Value.Replace(song_url,"");
            tmp = getLoaction(index_url+music_id);
            music_url = UnLockLoactionToString(tmp);
            music_name = m2.Value.Replace("的歌曲|" + song_url, "");
            map.Add(music_url,music_name);
            m1 = m1.NextMatch();
            m2 = m2.NextMatch();

        }

    }

    public String getMax(string str) //获取最大的页数
    {
        Match m1 = new Regex("class=\"p_num\">\\S*</a> <a class=\"p_redirect_l\"").Match(str);
        while(m1.Success)
        {
            tmp = m1.Value.Replace("class=\"p_num\">|</a>|<a class=\"p_redirect_l\"", "").Replace(" ", "") ;
            m1 = m1.NextMatch();
        }
        return tmp;
    }

    public void getMusicLink(string url_name , LinkedList<string> list) //获取歌曲的id
    {
        code = printURL(url_name);
        Match m1 = new Regex("href=\"/song/(.+?\\S*\")").Match(code);
        while(m1.Success)
        {
            tmp = m1.Value.Replace("href=\"|\"", "");
            list.AddFirst(xiami+tmp);
            m1 = m1.NextMatch();
        }
    }

    public String getPage(string str) //获取一共有多少页
    {
        Match m1 = new Regex("共\\d*条").Match(str);
        while(m1.Success)
        {
            tmp = m1.Value.Replace("共|条", "");
            m1 = m1.NextMatch();
        }
        return tmp;
    }

    public void getTopMusicLink(String url_name, LinkedList<String> list) //获取top歌的链接
    {
        code = printURL(url_name);
        //		System.Console.WriteLine(code);

        Match m1 = new Regex("href=\"(.+?/song/\\S*\")").Match(code);
        while (m1.Success)
        {
            tmp = m1.Value.Replace("href=\"|\"", "");
            list.AddFirst("https:" + tmp);
            m1 = m1.NextMatch();
        }
    }

    public void sleep(int time) //休眠用的
    {
        System.Threading.Thread.Sleep(time);
    }

    public void search_music(String search_name, LinkedList<String> list) //搜索歌曲
    {
        tmp = search_name;
        search_name = HttpUtility.UrlEncode(tmp, System.Text.Encoding.UTF8); //需要进行encode转义，不然没法用
        String search_url = "https://www.xiami.com/search/song/page/1?key=" + search_name + "&category=-1";
        code = printURL(search_url);
        int max_page = int.Parse(getPage(code));
        int max_num = max_page / 20;
        System.Console.WriteLine("一共找到: " + max_page + " 首歌曲\n一共有: " + max_num + " 页");
        sleep(new Random().Next(2000,4000));
        for (int i = 1; i <= max_num; i++)
        {
            System.Console.WriteLine("开始操作第  " + i + " 页");
            search_url = "https://www.xiami.com/search/song/page/" + i + "?key=" + search_name + "&category=-1";
            code = printURL(search_url);
            Match m1 = new Regex("href=\"//www.xiami.com/song/(.+?\\S*>)").Match(code);
            while (m1.Success)
            {
                tmp = m1.Value;
                Match m2 = new Regex("href=\"(.+?\\S*\")").Match(tmp);
                if (m2.Success)
                {
                    music_url = "https:" + m2.Value.Replace("href=\"|\"", "");
                    list.AddFirst(music_url);
                    m2 = m2.NextMatch();
                }
                m1 = m1.NextMatch();
            }
            System.Console.WriteLine("第  " + i + " 页捕获完成(如果速度过快的话,很可能嗝屁了)\n延迟了: " + (new Random().Next(2000, 4000) / 1000) + " 秒");
            sleep(new Random().Next(2000, 4000));
        }



    }

    public void start(String url_name, String save_path)
    {
        LinkedList<String> list = new LinkedList<string>();
        Dictionary<string, string> map = new Dictionary<string, string>();
        max = getMax(printURL(url_name));
        int v = 0;
        String page_name = "?page=";
        int m = int.Parse(max);
        Console.WriteLine("获取链接中。。。。");
        for(int i =1;i<=m;i++)
        {
            getMusicLink(url_name+page_name+i,list);
        }
        System.Console.WriteLine("获取歌曲信息中。。。。");
        System.Console.WriteLine("一共有: " + list.Count + " 首歌曲");
        foreach(string s in list)
        {
            getInfo(s,map);
            System.Console.WriteLine("已经捕获到了第  " + (v + 1) + " 首歌曲的位置");
            v++;
        }
        System.Console.WriteLine("获取歌曲信息OK");
        foreach(KeyValuePair<string,string> e in map)
        {
            music_name = e.Value;
            music_url = e.Key;
            System.Console.WriteLine("开始下载: " + music_name);
            down(music_url,save_path,music_name.Replace(" ", "-"));
            System.Console.WriteLine("下载完成: " + music_name);
            num++;
        }
        System.Console.WriteLine("一共下载了 : " + num + "首歌曲");




    }

    public void start_list(String url_name, String save_path)
    {
        LinkedList<String> list = new LinkedList<string>();
        Dictionary<string, string> map = new Dictionary<string, string>();
        max = getMax(printURL(url_name));
        int v = 0;
        String page_name = "?page=";
        int m = int.Parse(max);
        Console.WriteLine("获取链接中。。。。");
        for (int i = 1; i <= m; i++)
        {
            getMusicLink(url_name + page_name + i, list);
        }
        System.Console.WriteLine("获取歌曲信息中。。。。");
        System.Console.WriteLine("一共有: " + list.Count + " 首歌曲");
        foreach (string s in list)
        {
            getInfo(s, map);
            System.Console.WriteLine("已经捕获到了第  " + (v + 1) + " 首歌曲的位置");
            v++;
        }
        System.Console.WriteLine("获取歌曲信息OK");
        foreach (KeyValuePair<string, string> e in map)
        {
            music_name = e.Value;
            music_url = e.Key;
            System.Console.WriteLine("开始下载: " + music_name);
            down(music_url, save_path, music_name.Replace(" ", "-"));
            System.Console.WriteLine("下载完成: " + music_name);
            num++;
        }
        System.Console.WriteLine("一共下载了 : " + num + "首歌曲");




    }

    public void start_list_(String url_name, String save_path)
    {
        LinkedList<String> list = new LinkedList<string>();
        Dictionary<string, string> map = new Dictionary<string, string>();
        Dictionary<int, KeyValuePair<string,string>> tmp_map = new Dictionary<int, KeyValuePair<string, string>>();
        max = getMax(printURL(url_name));
        int v = 0;
        String page_name = "?page=";
        int m = int.Parse(max);
        Console.WriteLine("获取链接中。。。。");
        for (int i = 1; i <= m; i++)
        {
            getMusicLink(url_name + page_name + i, list);
        }
        System.Console.WriteLine("获取歌曲信息中。。。。");
        System.Console.WriteLine("一共有: " + list.Count + " 首歌曲");
        foreach (string s in list)
        {
            getInfo(s, map);
            System.Console.WriteLine("已经捕获到了第  " + (v + 1) + " 首歌曲的位置");
            v++;
        }
        System.Console.WriteLine("获取歌曲信息OK");
        v = 1;
        foreach (KeyValuePair<string, string> e in map)
        {
            tmp_map.Add(v,e);
            v++;
        }

        if(tmp_map.Count >1)
        {
            nn = 0;
            foreach(KeyValuePair<int,KeyValuePair<string,string>> e in tmp_map)
            {
                Console.WriteLine(e.Key + " -- " + e.Value.Value + "\t");
                if(nn == 10)
                {
                    Console.WriteLine();
                    nn = 0;
                }
                nn++;
            }
            Console.WriteLine("请输入需要下载的歌曲数目: ");
            int numm = int.Parse(ss);
            if(numm > 1)
            {
                for(int i = 0;i<numm;i++)
                {
                    Console.WriteLine("\n\n=======>请选择需要进行下载的歌曲序号(1-9): ");
                    int ii = int.Parse(ss);
                    foreach(KeyValuePair<int ,KeyValuePair<string,string>> e in tmp_map)
                    {
                        if(ii == e.Key)
                        {
                            music_url = e.Value.Key;
                            music_name = e.Value.Value;
                        }
                    }
                    if(music_name.Length>1|music_url.Length>1)
                    {
                        Console.WriteLine("开始下载: " + music_name);
                        down(music_url,save_path,music_name.Replace(" ", "-"));
                        Console.WriteLine("下载完成: " + music_name);
                    }
                    else
                    {
                        Console.WriteLine("可能出了点偏差");
                    }


                }
            }
            else
            {
                Console.WriteLine("请选择一首歌曲");
            }
            Console.WriteLine("一共下载了 : " + num + "首歌曲");

        }
        else
        {
            System.Console.WriteLine("没有获取到任何内容,可能是又被封禁了,请明天开始使用!");
        }


       

    }

    public void start_list_list(String url_name, String save_path)
    {
        LinkedList<String> list = new LinkedList<string>();
        Dictionary<string, string> map = new Dictionary<string, string>();
        Dictionary<int, KeyValuePair<string, string>> tmp_map = new Dictionary<int, KeyValuePair<string, string>>();
        max = getMax(printURL(url_name));
        int v = 0;
        String page_name = "?page=";
        int m = int.Parse(max);
        Console.WriteLine("获取链接中。。。。");
        for (int i = 1; i <= m; i++)
        {
            getMusicLink(url_name + page_name + i, list);
        }
        System.Console.WriteLine("获取歌曲信息中。。。。");
        System.Console.WriteLine("一共有: " + list.Count + " 首歌曲");
        foreach (string s in list)
        {
            getInfo(s, map);
            System.Console.WriteLine("已经捕获到了第  " + (v + 1) + " 首歌曲的位置");
            v++;
        }
        System.Console.WriteLine("获取歌曲信息OK");
        v = 1;
        foreach (KeyValuePair<string, string> e in map)
        {
            tmp_map.Add(v, e);
            v++;
        }

        if (tmp_map.Count > 1)
        {
            nn = 0;
            foreach (KeyValuePair<int, KeyValuePair<string, string>> e in tmp_map)
            {
                Console.WriteLine(e.Key + " -- " + e.Value.Value + "\t");
                if (nn == 10)
                {
                    Console.WriteLine();
                    nn = 0;
                }
                nn++;
            }
            Console.WriteLine("请输入需要下载的歌曲数目: ");
            int numm = int.Parse(ss);
            if (numm > 1)
            {
                for (int i = 0; i < numm; i++)
                {
                    Console.WriteLine("\n\n=======>请选择需要进行下载的歌曲序号(1-9): ");
                    int ii = int.Parse(ss);
                    foreach (KeyValuePair<int, KeyValuePair<string, string>> e in tmp_map)
                    {
                        if (ii == e.Key)
                        {
                            music_url = e.Value.Key;
                            music_name = e.Value.Value;
                        }
                    }
                    if (music_name.Length > 1 | music_url.Length > 1)
                    {
                        Console.WriteLine("开始下载: " + music_name);
                        down(music_url, save_path, music_name.Replace(" ", "-"));
                        Console.WriteLine("下载完成: " + music_name);
                    }
                    else
                    {
                        Console.WriteLine("可能出了点偏差");
                    }


                }
            }
            else
            {
                Console.WriteLine("请选择一首歌曲");
            }
            Console.WriteLine("一共下载了 : " + num + "首歌曲");

        }
        else
        {
            System.Console.WriteLine("没有获取到任何内容,可能是又被封禁了,请明天开始使用!");
        }




    }

    public void start_search(String search_name, String save_path)
    {
        LinkedList<String> list = new LinkedList<string>();
        Dictionary<string, string> map = new Dictionary<string, string>();
        search_music(search_name,list);
        foreach (string s in list)
        {
            getInfo(s, map);
        }
        foreach (KeyValuePair<string, string> e in map)
        {
            music_name = e.Value;
            music_url = e.Key;
            System.Console.WriteLine("开始下载: " + music_name);
            down(music_url, save_path, music_name.Replace(" ", "-"));
            System.Console.WriteLine("下载完成: " + music_name);
            num++;
        }
        System.Console.WriteLine("一共下载了 : " + num + "首歌曲");


    }

    public void start_search_list(String search_name, String save_path)
    {
        LinkedList<String> list = new LinkedList<string>();
        Dictionary<string, string> map = new Dictionary<string, string>();
        Dictionary<int, KeyValuePair<string, string>> tmp_map = new Dictionary<int, KeyValuePair<string, string>>();
        int v = 0;
        search_music(search_name,list);
        foreach (string s in list)
        {
            getInfo(s, map);
           
        }
        System.Console.WriteLine("获取歌曲信息OK");
        v = 1;
        foreach (KeyValuePair<string, string> e in map)
        {
            tmp_map.Add(v, e);
            v++;
        }

        if (tmp_map.Count > 1)
        {
            nn = 0;
            foreach (KeyValuePair<int, KeyValuePair<string, string>> e in tmp_map)
            {
                Console.WriteLine(e.Key + " -- " + e.Value.Value + "\t");
                if (nn == 10)
                {
                    Console.WriteLine();
                    nn = 0;
                }
                nn++;
            }
            Console.WriteLine("请输入需要下载的歌曲数目: ");
            int numm = int.Parse(ss);
            if (numm > 1)
            {
                for (int i = 0; i < numm; i++)
                {
                    Console.WriteLine("\n\n=======>请选择需要进行下载的歌曲序号(1-9): ");
                    int ii = int.Parse(ss);
                    foreach (KeyValuePair<int, KeyValuePair<string, string>> e in tmp_map)
                    {
                        if (ii == e.Key)
                        {
                            music_url = e.Value.Key;
                            music_name = e.Value.Value;
                        }
                    }
                    if (music_name.Length > 1 | music_url.Length > 1)
                    {
                        Console.WriteLine("开始下载: " + music_name);
                        down(music_url, save_path, music_name.Replace(" ", "-"));
                        Console.WriteLine("下载完成: " + music_name);
                        num++;
                    }
                    else
                    {
                        Console.WriteLine("可能出了点偏差");
                    }


                }
            }
            else
            {
                Console.WriteLine("请选择一首歌曲");
            }
            Console.WriteLine("一共下载了 : " + num + "首歌曲");

        }
        else
        {
            System.Console.WriteLine("没有获取到任何内容,可能是又被封禁了,请明天开始使用!");
        }




    }

    public void start_Top(String url_name, String save_path)
    {
        LinkedList<String> list = new LinkedList<string>();
        Dictionary<string, string> map = new Dictionary<string, string>();
        max = getMax(printURL(url_name));
        int v = 0;
        String page_name = "?page=";
        int m = int.Parse(max);
        Console.WriteLine("获取链接中。。。。");
        for (int i = 1; i <= m; i++)
        {
            getTopMusicLink(url_name + page_name + i, list);
        }
        System.Console.WriteLine("获取歌曲信息中。。。。");
        System.Console.WriteLine("一共有: " + list.Count + " 首歌曲");
        foreach (string s in list)
        {
            getInfo(s, map);
            System.Console.WriteLine("已经捕获到了第  " + (v + 1) + " 首歌曲的位置");
            v++;
        }
        System.Console.WriteLine("获取歌曲信息OK");
        foreach (KeyValuePair<string, string> e in map)
        {
            music_name = e.Value;
            music_url = e.Key;
            System.Console.WriteLine("开始下载: " + music_name);
            down(music_url, save_path, music_name.Replace(" ", "-"));
            System.Console.WriteLine("下载完成: " + music_name);
            num++;
        }
        System.Console.WriteLine("一共下载了 : " + num + "首歌曲");




    }

    public void start_Top_list(String url_name, String save_path)
    {
        LinkedList<String> list = new LinkedList<string>();
        Dictionary<string, string> map = new Dictionary<string, string>();
        Dictionary<int, KeyValuePair<string, string>> tmp_map = new Dictionary<int, KeyValuePair<string, string>>();
        max = getMax(printURL(url_name));
        int v = 0;
        String page_name = "?page=";
        int m = int.Parse(max);
        Console.WriteLine("获取链接中。。。。");
        for (int i = 1; i <= m; i++)
        {
            getTopMusicLink(url_name + page_name + i, list);
        }
        System.Console.WriteLine("获取歌曲信息中。。。。");
        System.Console.WriteLine("一共有: " + list.Count + " 首歌曲");
        foreach (string s in list)
        {
            getInfo(s, map);
            System.Console.WriteLine("已经捕获到了第  " + (v + 1) + " 首歌曲的位置");
            v++;
        }
        System.Console.WriteLine("获取歌曲信息OK");
        v = 1;
        foreach (KeyValuePair<string, string> e in map)
        {
            tmp_map.Add(v, e);
            v++;
        }

        if (tmp_map.Count > 1)
        {
            nn = 0;
            foreach (KeyValuePair<int, KeyValuePair<string, string>> e in tmp_map)
            {
                Console.WriteLine(e.Key + " -- " + e.Value.Value + "\t");
                if (nn == 10)
                {
                    Console.WriteLine();
                    nn = 0;
                }
                nn++;
            }
            Console.WriteLine("请输入需要下载的歌曲数目: ");
            int numm = int.Parse(ss);
            if (numm > 1)
            {
                for (int i = 0; i < numm; i++)
                {
                    Console.WriteLine("\n\n=======>请选择需要进行下载的歌曲序号(1-9): ");
                    int ii = int.Parse(ss);
                    foreach (KeyValuePair<int, KeyValuePair<string, string>> e in tmp_map)
                    {
                        if (ii == e.Key)
                        {
                            music_url = e.Value.Key;
                            music_name = e.Value.Value;
                        }
                    }
                    if (music_name.Length > 1 | music_url.Length > 1)
                    {
                        Console.WriteLine("开始下载: " + music_name);
                        down(music_url, save_path, music_name.Replace(" ", "-"));
                        Console.WriteLine("下载完成: " + music_name);
                    }
                    else
                    {
                        Console.WriteLine("可能出了点偏差");
                    }
                }
            }
            else
            {
                Console.WriteLine("请选择一首歌曲");
            }
            Console.WriteLine("一共下载了 : " + num + "首歌曲");

        }
        else
        {
            System.Console.WriteLine("没有获取到任何内容,可能是又被封禁了,请明天开始使用!");
        }
    }



}

class Music :t //这个是个汇总函数
{
    //file_path : 这个是存放文件保存路径的，默认为e:\\test\\files\\music\\

    private string file_path = "C:\\Windows\\xxxxxxx.txt" , save_path_mo = "e:\\test\\files\\music\\", se = " ",t = "", y_n = " ";
    private bool fl = false;

    public void p(object obj)
    {
        System.Console.WriteLine(obj);
    }
    public bool ex()
    {
        return File.Exists(file_path);
    }

    public bool setSave(string s)
    {
        
        if(!ex())
        {
            StreamWriter write = new StreamWriter(new FileStream(file_path,FileMode.Create));
            write.Write(s);
            write.Close();
            fl = true;
        }
        else
        {
            fl = false;
        }

        return fl;
    }

    public void sleep(int time)
    {
        System.Threading.Thread.Sleep(time);
    }

    public string getPath() //获取文件保存路径
    {
        StreamReader read = new StreamReader(file_path,Encoding.Default);
        while((tmp = read.ReadLine()) !=null)
        {
            se += tmp;
        }
        read.Close();
        return se;
    }

    public void kg_help() //酷狗音乐的帮助函数
    {
        if(!ex())
        {
            p("您是否要使用默认的保存路径(y|n)");
            y_n = (string)ss;
            switch (y_n)
            {
                case "n":
                case "N":
                    p("请输入您自定义的路径,推荐c盘跟d盘:\n");
                    t = (string)ss;
                    if (setSave(t))
                    {
                        p("设置成功!");
                    }
                    else
                    {
                        p("设置失败,极有可能是没有权限");
                        return;
                    }
                    break;
                
            }
        }
        if (ex())
        {
            save_path_mo = getPath();
        }
        new cool_dog().init();
            p("启动酷狗功能......");
            p("您当前的保存路径为: " + save_path_mo + " (如果没有任何信息显示出来，请用管理员身份运行)");
            p("1.下载一个歌手的所有歌曲\n2.选择歌手的部分歌曲进行下载\n3.下载一个歌单里的所有歌曲\n4.选择歌单的部分歌曲进行下载\n5.下载搜索内容的所有歌曲\n6.选择下载搜索内容的歌曲\n88.退出\n请选择操作项: ");

            se = (string)ss;
            switch (se)
            {
                case "1":
                    p("请把某个歌手的链接复制到这里:\n");
                    t = (string)ss;
                    if (t.Length < 7)
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        kg_help();
                    }
                    else if (t.Equals(""))
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        kg_help();
                    }
                    else
                    {
                        p("正在进行操作,请稍后...");
                        new cool_dog().start(save_path_mo, t);
                        p("是否继续操作别的(y|n)");
                        y_n = (string)ss;
                        switch (y_n)
                        {
                            case "y":
                            case "Y":
                                kg_help();
                                break;
                            default:
                                p("谢谢使用!");
                                break;
                        }
                    }
                    break;
                case "2":
                    p("请把某个歌手的链接复制到这里:\n");
                    t = (string)ss;
                    if (t.Length < 7)
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        kg_help();
                    }
                    else if (t.Equals(""))
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        kg_help();
                    }
                    else
                    {
                        p("这个功能需要消耗比较大的资源,而且速度较慢,还请耐心等待一会......");
                        p("正在进行操作,请稍后...");
                        new cool_dog().start_start_sel(save_path_mo, t);
                        p("是否继续操作别的(y|n)");
                        y_n = (string)ss;
                        switch (y_n)
                        {
                            case "y":
                            case "Y":
                                kg_help();
                                break;
                            default:
                                p("谢谢使用!");
                                break;
                        }
                    }

                    break;
                case "3":
                    p("请把某个歌单的链接复制到这里:\n");
                    t = (string)ss;
                    if (t.Length < 7)
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        kg_help();
                    }
                    else if (t.Equals(""))
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        kg_help();
                    }
                    else
                    {
                        p("正在进行操作,请稍后...");
                        new cool_dog().start_list(t, save_path_mo);
                        p("是否继续操作别的(y|n)");
                        y_n = (string)ss;
                        switch (y_n)
                        {
                            case "y":
                            case "Y":
                                kg_help();
                                break;
                            default:
                                p("谢谢使用!");
                                break;
                        }
                    }
                    break;
                case "4":
                    p("请把某个歌单的链接复制到这里:\n");
                    t = (string)ss;
                    if (t.Length < 7)
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        kg_help();
                    }
                    else if (t.Equals(""))
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        kg_help();
                    }
                    else
                    {
                        p("这个功能需要消耗比较大的资源,而且速度较慢,还请耐心等待一会......");
                        p("正在进行操作,请稍后...");
                        new cool_dog().start_list_sel(t, save_path_mo);
                        p("是否继续操作别的(y|n)");
                        y_n = (string)ss;
                        switch (y_n)
                        {
                            case "y":
                            case "Y":
                                kg_help();
                                break;
                            default:
                                p("谢谢使用!");
                                break;
                        }
                    }
                    break;
                case "5":
                    p("请输入需要进行搜索的内容:\n");
                    t = (string)ss;
                    if (t.Length < 1)
                    {
                        p("请确认是合法的名称!(自动暂停3秒)");
                        sleep(3);
                        kg_help();
                    }
                    else if (t.Equals(""))
                    {
                        p("请确认是合法的名称!(自动暂停3秒)");
                        sleep(3);
                        kg_help();
                    }
                    else
                    {
                        p("正在进行操作,请稍后...");
                        new cool_dog().start_search(t, save_path_mo);
                        p("是否继续操作别的(y|n)");
                        y_n = (string)ss;
                        switch (y_n)
                        {
                            case "y":
                            case "Y":
                                kg_help();
                                break;
                            default:
                                p("谢谢使用!");
                                break;
                        }
                    }
                    break;
                case "6":
                    p("请输入需要进行搜索的内容:\n");
                    t = (string)ss;
                    if (t.Length < 1)
                    {
                        p("请确认是合法的名称!(自动暂停3秒)");
                        sleep(3);
                        kg_help();
                    }
                    else if (t.Equals(""))
                    {
                        p("请确认是合法的名称!(自动暂停3秒)");
                        sleep(3);
                        kg_help();
                    }
                    else
                    {
                        p("这个功能需要消耗比较大的资源,而且速度较慢,还请耐心等待一会......");
                        p("正在进行操作,请稍后...");
                        new cool_dog().start_search_sel(t, save_path_mo);
                        p("是否继续操作别的(y|n)");
                        y_n = (string)ss;
                        switch (y_n)
                        {
                            case "y":
                            case "Y":
                                kg_help();
                                break;
                            default:
                                p("谢谢使用!");
                                break;
                        }
                    }
                    break;
                case "88":
                    p("谢谢使用!");
                    
                    break;
                default:
                //kg_help();
                Console.WriteLine(": " +se);
                sleep(3000);
                    break;
            }


        

    }

    public void music_163_help() //网易云的帮助函数
    {
        if (!ex())
        {
            p("您是否要使用默认的保存路径(y|n)");
            y_n = (string)ss;
            switch (y_n)
            {
                case "n":
                case "N":
                    p("请输入您自定义的路径,推荐c盘跟d盘:\n");
                    t = (string)ss;
                    if (setSave(t))
                    {
                        p("设置成功!");
                    }
                    else
                    {
                        p("设置失败,极有可能是没有权限");
                        return;
                    }
                    break;
                case "y":
                case "Y":
                    music_163_help();
                    break;
            }
        }
        if (ex())
        {
            save_path_mo = getPath();
        }
            new music_163().init();
            p("启动网易云功能......");
            p("您当前的保存路径为: " + save_path_mo + " (如果没有任何信息显示出来，请用管理员身份运行)");
            p("1.下载一个歌手的所有歌曲\n2.选择下载一个歌手的歌曲\n3.下载一个歌单里的所有歌曲\n4.选择下载一个歌单里的歌曲\n88.退出\n请选择操作项: ");
            se = (string)ss;
            switch (se)
            {
                case "1":
                    p("请把某个歌手的链接复制到这里:\n");
                    t = (string)ss;
                    if (t.Length < 7)
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        music_163_help();
                    }
                    else if (t.Equals(""))
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        music_163_help();
                    }
                    else
                    {
                        p("正在进行操作,请稍后...");
                        new music_163().start(t,save_path_mo);
                        p("是否继续操作别的(y|n)");
                        y_n = (string)ss;
                        switch (y_n)
                        {
                            case "y":
                            case "Y":
                                music_163_help();
                                break;
                            default:
                                p("谢谢使用!");
                                break;
                        }
                    }
                    break;
                case "2":
                    p("请把某个歌手的链接复制到这里:\n");
                    t = (string)ss;
                    if (t.Length < 7)
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        music_163_help();
                    }
                    else if (t.Equals(""))
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        music_163_help();
                    }
                    else
                    {
                        p("这个功能需要消耗比较大的资源,而且速度较慢,还请耐心等待一会......");
                        p("正在进行操作,请稍后...");
                        new music_163().start_sel(t,save_path_mo);
                        p("是否继续操作别的(y|n)");
                        y_n = (string)ss;
                        switch (y_n)
                        {
                            case "y":
                            case "Y":
                                kg_help();
                                break;
                            default:
                                p("谢谢使用!");
                                break;
                        }
                    }

                    break;
                case "3":
                    p("请把某个歌单的链接复制到这里:\n");
                    t = (string)ss;
                    if (t.Length < 7)
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        music_163_help();
                    }
                    else if (t.Equals(""))
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        music_163_help();
                    }
                    else
                    {
                        p("正在进行操作,请稍后...");
                        new music_163().start(t, save_path_mo);
                        p("是否继续操作别的(y|n)");
                        y_n = (string)ss;
                        switch (y_n)
                        {
                            case "y":
                            case "Y":
                                music_163_help();
                                break;
                            default:
                                p("谢谢使用!");
                                break;
                        }
                    }
                    break;
                case "4":
                    p("请把某个歌单的链接复制到这里:\n");
                    t = (string)ss;
                    if (t.Length < 7)
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        music_163_help();
                    }
                    else if (t.Equals(""))
                    {
                        p("请确认是合法的链接!(自动暂停3秒)");
                        sleep(3);
                        music_163_help();
                    }
                    else
                    {
                        p("这个功能需要消耗比较大的资源,而且速度较慢,还请耐心等待一会......");
                        p("正在进行操作,请稍后...");
                        new music_163().start_sel(t, save_path_mo);
                        p("是否继续操作别的(y|n)");
                        y_n = (string)ss;
                        switch (y_n)
                        {
                            case "y":
                            case "Y":
                                music_163_help();
                                break;
                            default:
                                p("谢谢使用!");
                                break;
                        }
                    }
                    break;
               
                case "88":
                    p("谢谢使用!");
                    
                    break;
                default:
                    music_163_help();
                    break;
            }


        

    }

    public void xiami_help() //虾米的帮助函数
    {
        if (!ex())
        {
            p("您是否要使用默认的保存路径(y|n)");
            y_n = (string)ss;
            switch (y_n)
            {
                case "n":
                case "N":
                    p("请输入您自定义的路径,推荐c盘跟d盘:\n");
                    t = (string)ss;
                    if (setSave(t))
                    {
                        p("设置成功!");
                    }
                    else
                    {
                        p("设置失败,极有可能是没有权限");
                        return;
                    }
                    break;
               
            }
        }
        if(ex())
        {
            save_path_mo = getPath();
        }

        new xiami_music().init();
        p("启动虾米音乐功能......\n虾米音乐容易fuck,目前还没有进行修复\n");
        p("您当前的保存路径为: " + save_path_mo + " (如果没有任何信息显示出来，请用管理员身份运行)");
        p("1.下载一个歌手的所有歌曲\n2.选择歌手的部分歌曲进行下载\n3.下载一个歌手的所有top歌曲\n4.选择一个歌手的部分top歌曲\n5.下载一个歌单里的所有歌曲\n6.选择歌单的部分歌曲进行下载\n7.下载搜索内容的所有歌曲\n8.选择下载搜索内容的歌曲\n88.退出\n请选择操作项: ");
        se = (string)ss;
        switch (se)
        {
            case "1":
                p("请把某个歌手的链接复制到这里:\n");
                t = (string)ss;
                if (t.Length < 7)
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else if (t.Equals(""))
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else
                {
                    p("正在进行操作,请稍后...");
                    new xiami_music().start(t, save_path_mo);
                    p("是否继续操作别的(y|n)");
                    y_n = (string)ss;
                    switch (y_n)
                    {
                        case "y":
                        case "Y":
                            xiami_help();
                            break;
                        default:
                            p("谢谢使用!");
                            break;
                    }
                }
                break;
            case "2":
                p("请把某个歌手的链接复制到这里:\n");
                t = (string)ss;
                if (t.Length < 7)
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else if (t.Equals(""))
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else
                {
                    p("这个功能需要消耗比较大的资源,而且速度较慢,还请耐心等待一会......");
                    p("正在进行操作,请稍后...");
                    new xiami_music().start_list_(t, save_path_mo);
                    p("是否继续操作别的(y|n)");
                    y_n = (string)ss;
                    switch (y_n)
                    {
                        case "y":
                        case "Y":
                            kg_help();
                            break;
                        default:
                            p("谢谢使用!");
                            break;
                    }
                }

                break;
            case "3":
                p("请把某个歌手的top链接复制到这里:\n");
                t = (string)ss;
                if (t.Length < 7)
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else if (t.Equals(""))
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else
                {
                    p("正在进行操作,请稍后...");
                    new xiami_music().start_Top(t, save_path_mo);
                    p("是否继续操作别的(y|n)");
                    y_n = (string)ss;
                    switch (y_n)
                    {
                        case "y":
                        case "Y":
                            xiami_help();
                            break;
                        default:
                            p("谢谢使用!");
                            break;
                    }
                }
                break;
            case "4":
                p("请把某个歌手的top链接复制到这里:\n");
                t = (string)ss;
                if (t.Length < 7)
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else if (t.Equals(""))
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else
                {
                    p("正在进行操作,请稍后...");
                    new xiami_music().start_Top_list(t, save_path_mo);
                    p("是否继续操作别的(y|n)");
                    y_n = (string)ss;
                    switch (y_n)
                    {
                        case "y":
                        case "Y":
                            xiami_help();
                            break;
                        default:
                            p("谢谢使用!");
                            break;
                    }
                }
                break;
            case "5":
                p("请把某个歌单的链接复制到这里:\n");
                t = (string)ss;
                if (t.Length < 7)
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else if (t.Equals(""))
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else
                {
                    p("这个功能需要消耗比较大的资源,而且速度较慢,还请耐心等待一会......");
                    p("正在进行操作,请稍后...");
                    new xiami_music().start_list(t, save_path_mo);
                    p("是否继续操作别的(y|n)");
                    y_n = (string)ss;
                    switch (y_n)
                    {
                        case "y":
                        case "Y":
                            xiami_help();
                            break;
                        default:
                            p("谢谢使用!");
                            break;
                    }
                }
                break;

            case "6":
                p("请把某个歌单的链接复制到这里:\n");
                t = (string)ss;
                if (t.Length < 7)
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else if (t.Equals(""))
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else
                {
                    p("这个功能需要消耗比较大的资源,而且速度较慢,还请耐心等待一会......");
                    p("正在进行操作,请稍后...");
                    new xiami_music().start_list_list(t, save_path_mo);
                    p("是否继续操作别的(y|n)");
                    y_n = (string)ss;
                    switch (y_n)
                    {
                        case "y":
                        case "Y":
                            xiami_help();
                            break;
                        default:
                            p("谢谢使用!");
                            break;
                    }
                }
                break;
            case "7":
                p("请输入需要进行搜索的内容:\n");
                t = (string)ss;
                if (t.Length < 7)
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else if (t.Equals(""))
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else
                {
                    p("这个功能需要消耗比较大的资源,而且速度较慢,还请耐心等待一会......");
                    p("正在进行操作,请稍后...");
                    new xiami_music().start_search(t, save_path_mo);
                    p("是否继续操作别的(y|n)");
                    y_n = (string)ss;
                    switch (y_n)
                    {
                        case "y":
                        case "Y":
                            xiami_help();
                            break;
                        default:
                            p("谢谢使用!");
                            break;
                    }
                }
                break;
            case "8":
                p("请输入需要进行搜索的内容:\n");
                t = (string)ss;
                if (t.Length < 7)
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else if (t.Equals(""))
                {
                    p("请确认是合法的链接!(自动暂停3秒)");
                    sleep(3);
                    xiami_help();
                }
                else
                {
                    p("这个功能需要消耗比较大的资源,而且速度较慢,还请耐心等待一会......");
                    p("正在进行操作,请稍后...");
                    new xiami_music().start_search_list(t, save_path_mo);
                    p("是否继续操作别的(y|n)");
                    y_n = (string)ss;
                    switch (y_n)
                    {
                        case "y":
                        case "Y":
                            xiami_help();
                            break;
                        default:
                            p("谢谢使用!");
                            break;
                    }
                }
                break;
            case "88":
                p("谢谢使用!");

                break;
            default:
                xiami_help();
                break;
        }



    }

    public void show_help()
    {
        p("音乐工具集合: ");
        p("	-k : kugou	|	酷狗音乐	|	指定酷狗音乐来操作");
        p("	-w : music_163	|	网易云音乐	|	指定网易音乐来操作");
        p("	-x : xiami	|	虾米音乐	|	指定虾米音乐来操作");
        p("	-h : help	|  列出所有可以用的帮助 |	list all hellp\n");
        p("	-del : 移除配置路径的文件 ");
        p("	\n	更多音乐类型的会在未来进行加入!\n\n");
    }

    public void start_help(string[] arr) //命令行参数用的
    {
        if (arr.Length == 0)
        {
            p("run -h");
        }
        else
        {
            switch (arr[0])
            {
                case "-k":
                    kg_help();
                    break;
                case "-w":
                    //new music_163_help().run_c_163();
                    music_163_help();
                    break;
                case "-x":
                    //new xiami_help().run_xm();
                    xiami_help();
                    break;
                case "-del":
                    p("正在移除配置文件...");
                    File.Delete(file_path);
                    p("移除完成!");
                    break;
                case "-h":
                    //new show_help().show_Help();
                    show_help();
                    break;
                default:
                    p("请运行-h");
                    break;
            }
        }
    }

}

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            //开始你想要做的事情！
        }
    }
}
