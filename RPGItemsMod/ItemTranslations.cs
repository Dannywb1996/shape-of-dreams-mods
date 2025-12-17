using System.Collections.Generic;

/// <summary>
/// Multi-language translations for all item names and descriptions
/// This file provides lookup tables for translating items to various languages
/// </summary>
public static class ItemTranslations
{
    private static Dictionary<int, string> _nameCN = null;
    private static Dictionary<int, string> _descCN = null;
    
    // Polish translations
    private static Dictionary<int, string> _namePL = null;
    private static Dictionary<int, string> _descPL = null;
    
    // German translations
    private static Dictionary<int, string> _nameDE = null;
    private static Dictionary<int, string> _descDE = null;
    
    // French translations
    private static Dictionary<int, string> _nameFR = null;
    private static Dictionary<int, string> _descFR = null;
    
    // Spanish translations
    private static Dictionary<int, string> _nameES = null;
    private static Dictionary<int, string> _descES = null;
    
    // Portuguese translations
    private static Dictionary<int, string> _namePT = null;
    private static Dictionary<int, string> _descPT = null;
    
    // Russian translations
    private static Dictionary<int, string> _nameRU = null;
    private static Dictionary<int, string> _descRU = null;
    
    // Japanese translations
    private static Dictionary<int, string> _nameJA = null;
    private static Dictionary<int, string> _descJA = null;
    
    // Korean translations
    private static Dictionary<int, string> _nameKO = null;
    private static Dictionary<int, string> _descKO = null;
    
    // Italian translations
    private static Dictionary<int, string> _nameIT = null;
    private static Dictionary<int, string> _descIT = null;
    
    // Turkish translations
    private static Dictionary<int, string> _nameTR = null;
    private static Dictionary<int, string> _descTR = null;
    
    /// <summary>
    /// Get translated name based on current language
    /// </summary>
    public static string GetName(int itemId)
    {
        string langCode = Localization.CurrentLanguageCode;
        
        // Chinese (Simplified and Traditional use same translations)
        if (langCode == "zh-CN" || langCode == "zh-TW")
            return GetNameCN(itemId);
        if (langCode == "pl-PL")
            return GetNamePL(itemId);
        if (langCode == "de-DE")
            return GetNameDE(itemId);
        if (langCode == "fr-FR")
            return GetNameFR(itemId);
        if (langCode == "es-MX")
            return GetNameES(itemId);
        if (langCode == "pt-BR")
            return GetNamePT(itemId);
        if (langCode == "ru-RU")
            return GetNameRU(itemId);
        if (langCode == "ja-JP")
            return GetNameJA(itemId);
        if (langCode == "ko-KR")
            return GetNameKO(itemId);
        if (langCode == "it-IT")
            return GetNameIT(itemId);
        if (langCode == "tr-TR")
            return GetNameTR(itemId);
        
        return null; // English fallback
    }
    
    /// <summary>
    /// Get translated description based on current language
    /// </summary>
    public static string GetDescription(int itemId)
    {
        string langCode = Localization.CurrentLanguageCode;
        
        // Chinese (Simplified and Traditional use same translations)
        if (langCode == "zh-CN" || langCode == "zh-TW")
            return GetDescCN(itemId);
        if (langCode == "pl-PL")
            return GetDescPL(itemId);
        if (langCode == "de-DE")
            return GetDescDE(itemId);
        if (langCode == "fr-FR")
            return GetDescFR(itemId);
        if (langCode == "es-MX")
            return GetDescES(itemId);
        if (langCode == "pt-BR")
            return GetDescPT(itemId);
        if (langCode == "ru-RU")
            return GetDescRU(itemId);
        if (langCode == "ja-JP")
            return GetDescJA(itemId);
        if (langCode == "ko-KR")
            return GetDescKO(itemId);
        if (langCode == "it-IT")
            return GetDescIT(itemId);
        if (langCode == "tr-TR")
            return GetDescTR(itemId);
        
        return null; // English fallback
    }
    
    public static string GetNameCN(int itemId)
    {
        if (_nameCN == null) Initialize();
        string result;
        if (_nameCN.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    public static string GetDescCN(int itemId)
    {
        if (_descCN == null) Initialize();
        string result;
        if (_descCN.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    // Polish
    public static string GetNamePL(int itemId)
    {
        if (_namePL == null) InitializePL();
        string result;
        if (_namePL.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    public static string GetDescPL(int itemId)
    {
        if (_descPL == null) InitializePL();
        string result;
        if (_descPL.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    // German
    public static string GetNameDE(int itemId)
    {
        if (_nameDE == null) InitializeDE();
        string result;
        if (_nameDE.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    public static string GetDescDE(int itemId)
    {
        if (_descDE == null) InitializeDE();
        string result;
        if (_descDE.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    // French
    public static string GetNameFR(int itemId)
    {
        if (_nameFR == null) InitializeFR();
        string result;
        if (_nameFR.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    public static string GetDescFR(int itemId)
    {
        if (_descFR == null) InitializeFR();
        string result;
        if (_descFR.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    // Spanish
    public static string GetNameES(int itemId)
    {
        if (_nameES == null) InitializeES();
        string result;
        if (_nameES.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    public static string GetDescES(int itemId)
    {
        if (_descES == null) InitializeES();
        string result;
        if (_descES.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    // Portuguese
    public static string GetNamePT(int itemId)
    {
        if (_namePT == null) InitializePT();
        string result;
        if (_namePT.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    public static string GetDescPT(int itemId)
    {
        if (_descPT == null) InitializePT();
        string result;
        if (_descPT.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    // Russian
    public static string GetNameRU(int itemId)
    {
        if (_nameRU == null) InitializeRU();
        string result;
        if (_nameRU.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    public static string GetDescRU(int itemId)
    {
        if (_descRU == null) InitializeRU();
        string result;
        if (_descRU.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    // Japanese
    public static string GetNameJA(int itemId)
    {
        if (_nameJA == null) InitializeJA();
        string result;
        if (_nameJA.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    public static string GetDescJA(int itemId)
    {
        if (_descJA == null) InitializeJA();
        string result;
        if (_descJA.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    // Korean
    public static string GetNameKO(int itemId)
    {
        if (_nameKO == null) InitializeKO();
        string result;
        if (_nameKO.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    public static string GetDescKO(int itemId)
    {
        if (_descKO == null) InitializeKO();
        string result;
        if (_descKO.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    // Italian
    public static string GetNameIT(int itemId)
    {
        if (_nameIT == null) InitializeIT();
        string result;
        if (_nameIT.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    public static string GetDescIT(int itemId)
    {
        if (_descIT == null) InitializeIT();
        string result;
        if (_descIT.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    // Turkish
    public static string GetNameTR(int itemId)
    {
        if (_nameTR == null) InitializeTR();
        string result;
        if (_nameTR.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    public static string GetDescTR(int itemId)
    {
        if (_descTR == null) InitializeTR();
        string result;
        if (_descTR.TryGetValue(itemId, out result)) return result;
        return null;
    }
    
    private static void Initialize()
    {
        _nameCN = new Dictionary<int, string>();
        _descCN = new Dictionary<int, string>();
        
        Add(1, "新手之爪", "月华奥术会新成员的简单爪子。奥蕾娜即使被驱逐后仍保留着它。");
        Add(2, "学者之爪", "在禁忌典籍上磨砺的利爪。它留下的抓痕低语着秘密。");
        Add(3, "月光之爪", "在月光下微微发光的爪子。它们将月之能量转化为毁灭性的攻击。");
        Add(4, "奥术剃刀", "刻有治愈符文的爪子——如今用于撕裂而非治愈。");
        Add(5, "流亡者之爪", "被放逐后秘密锻造。每一次挥砍都在提醒她会社夺走了什么。");
        Add(6, "异端之握", "曾属于另一位被驱逐贤者的爪子。他们的灵魂引导着奥蕾娜的攻击。");
        Add(7, "献祭之爪", "随着奥蕾娜生命力衰退而变得更锋利的爪子。会社认为这项研究太危险了。");
        Add(8, "霜缚之爪", "用会社最深处金库的冰霜冻结的爪子。它们的触碰能麻痹肉体和灵魂。");
        Add(9, "月之撕裂者", "用禁忌附魔强化。它们造成的每一道伤口都能治愈奥蕾娜的盟友。");
        Add(10, "光辉之爪", "被窃取的阳光祝福的爪子。奥蕾娜对光明魔法的研究激怒了日月两教。");
        Add(11, "余烬之爪", "用自愿梦者的生命力点燃的爪子。火焰永不熄灭。");
        Add(12, "暗影之爪", "浸泡在噩梦精华中的爪子。奥蕾娜学会了黑暗能治愈光明无法治愈之物。");
        Add(13, "冰川收割者", "由永恒之冰雕刻的爪子。它们冻结敌人的血液并保存奥蕾娜的生命力。");
        Add(14, "凤凰之爪", "在凤凰火焰中锻造并用奥蕾娜自己的血液淬火的爪子。它们燃烧着不灭的怒火。");
        Add(15, "虚空撕裂者", "能撕裂现实本身的爪子。会社恐惧奥蕾娜接下来会发现什么。");
        Add(16, "天界之爪", "注入纯净星光的爪子。每一击都是对诸神想要隐藏的知识的祈祷。");
        Add(17, "大贤者之爪", "会社大贤者的礼仪之爪。奥蕾娜将其作为他们虚伪的证据。");
        Add(18, "生命编织者之爪", "奥蕾娜的杰作——能窃取生命力而不摧毁它的爪子。会社称之为异端。");
        Add(19, "空白魔典", "等待填写的空白法术书。像铋晶本人一样，它的书页中蕴含着无限潜力。");
        Add(20, "学徒之书", "初学者的法术书。成为铋晶的盲女曾用手指描摹它的书页，梦想着魔法。");
        Add(21, "水晶法典", "书页由薄宝石制成的典籍。它与铋晶的晶体本质产生共鸣。");
        Add(22, "流浪者日记", "一本旅行日记，记录着盲女从未见过却总是知晓的观察。");
        Add(23, "宝石入门", "水晶魔法的基础指南。它的文字像铋晶的彩虹色身体一样闪烁。");
        Add(24, "盲者经典", "为看不见的人用凸起字母书写的典籍。铋晶通过触摸和记忆阅读它。");
        Add(25, "余烬之言典籍", "书页永恒燃烧的书。火焰用铋晶感受而非看见的颜色与她交谈。");
        Add(26, "冰封词典", "被永恒之冰包裹的典籍。它冰冷的书页保存着梦境开始前的知识。");
        Add(27, "光辉手稿", "散发内在光芒的书。在她成为铋晶之前，它引导盲女穿越黑暗。");
        Add(28, "暗影魔典", "吸收光线的典籍。它黑暗的书页包含着连铋晶都害怕阅读的秘密。");
        Add(29, "活体法术书", "一本选择与盲女融合的有意识典籍。在一起，她们成为了更伟大的存在。");
        Add(30, "棱镜法典", "将魔法分解成组成颜色的典籍。铋晶通过它折射的光芒看世界。");
        Add(31, "虚空编年史", "记载失落于黑暗者记忆的典籍。铋晶阅读他们的故事以纪念他们。");
        Add(32, "宝石之心魔典", "与盲女融合的原始法术书。它的书页随着晶体生命而跳动。");
        Add(33, "致盲之光典籍", "如此耀眼以至于能让看得见的人失明的书。对铋晶来说，它只是温暖。");
        Add(34, "霜缚年鉴", "被冻结在时间中的古老典籍。它的预言讲述了一颗像女孩一样行走的宝石。");
        Add(35, "无名法典", "这本书没有名字，就像女孩没有视力。在一起，她们找到了自己的身份。");
        Add(36, "炼狱经典", "燃烧着拒绝让失明定义自己的梦者热情的典籍。");
        Add(37, "新兵步枪", "皇家卫队新兵的标准配发。拉塞尔塔在训练第一周结束前就掌握了它。");
        Add(38, "猎人长枪", "可靠的狩猎步枪。拉塞尔塔在加入卫队前曾用类似的枪养家。");
        Add(39, "巡逻卡宾枪", "紧凑可靠。非常适合在王国边境长时间巡逻。");
        Add(40, "神射手之枪", "一把为重视精准胜过威力者设计的平衡步枪。");
        Add(41, "火药火枪", "老旧但可靠。黑火药的气味让拉塞尔塔想起他的第一次狩猎。");
        Add(42, "侦察连发枪", "用于侦察任务的轻型步枪。速度和隐蔽胜过原始威力。");
        Add(43, "皇家卫队步枪", "拉塞尔塔在服役期间携带的武器。每道划痕都诉说着一个故事。");
        Add(44, "狙击手之傲", "为从不失手者准备的精准步枪。拉塞尔塔的猩红之眼能看到他人看不到的。");
        Add(45, "燃烧长步枪", "装载火焰弹头。敌人在子弹命中后仍会燃烧很久。");
        Add(46, "霜咬卡宾枪", "发射冷却至绝对零度的子弹。目标在致命一击前会慢如蜗牛。");
        Add(47, "夜幕步枪", "在黑暗中狩猎的武器。它的射击如影子般无声。");
        Add(48, "破晓火枪", "被晨光祝福。它的射击能穿透黑暗和欺骗。");
        Add(49, "猩红之眼", "以拉塞尔塔传奇的目光命名。没有目标能逃脱这把步枪的视线。");
        Add(50, "皇家处刑者", "为卫队精英保留。拉塞尔塔在拯救王国后获得了这把步枪。");
        Add(51, "阳炎炮", "发射浓缩光线的步枪。每一发都是一个微型日出。");
        Add(52, "虚空猎人", "发射纯黑暗子弹的步枪。目标消失得无影无踪。");
        Add(53, "绝对零度", "存在中最冷的点。即使时间也在它面前冻结。");
        Add(54, "龙息", "装载爆炸燃烧弹的步枪。王国的敌人学会了恐惧它的怒吼。");
        Add(55, "击剑花剑", "米斯特年轻时的训练用剑。即使那时，她也超越了阿斯特里德的每一位教官。");
        Add(56, "贵族细剑", "与米斯特的贵族血统相配的细剑。轻盈、优雅、致命。");
        Add(57, "决斗者细剑", "为一对一战斗设计的细剑。米斯特从未输过决斗。");
        Add(58, "迅捷细剑", "轻如米斯特手臂延伸的细剑。");
        Add(59, "阿斯特里德细剑", "在阿斯特里德最好的铁匠铺中锻造。王国工艺的象征。");
        Add(60, "舞者细剑", "为将战斗视为艺术者准备的细剑。米斯特的动作如诗般优美。");
        Add(61, "炽焰细剑", "被火焰环绕的细剑。米斯特的愤怒如同她的决心一样炽热。");
        Add(62, "暗影细剑", "从黑暗中出击的细剑。敌人在看到米斯特移动前就已倒下。");
        Add(63, "霜咬细剑", "冰冻钢铁的细剑。它的触碰能麻痹身体和灵魂。");
        Add(64, "卡米拉的恩典", "来自米斯特珍视之人的礼物。她为纪念他们的记忆而战。");
        Add(65, "格挡细剑", "为防守和进攻设计的细剑。米斯特将每次攻击都变成机会。");
        Add(66, "光辉细剑", "散发内在光芒的细剑。它能穿透谎言和阴影。");
        Add(67, "炼狱细剑", "燃烧着不灭热情的细剑。米斯特的决心无法被熄灭。");
        Add(68, "午夜细剑", "在绝对黑暗中锻造的细剑。它以死亡的寂静出击。");
        Add(69, "寒冬细剑", "由永恒之冰雕刻的细剑。它的寒冷只有米斯特在战斗中的专注能匹敌。");
        Add(70, "不屈细剑", "阿斯特里德最伟大决斗者的传奇细剑。米斯特通过无数胜利赢得了这个头衔。");
        Add(71, "树苗法杖", "来自灵界森林的年轻树枝。即使是树苗也响应娜琪亚的召唤。");
        Add(72, "唤灵者", "雕刻来引导森林精灵声音的法杖。它们向娜琪亚低语秘密。");
        Add(73, "森林之枝", "仍在生长的活树枝。森林的魔力流经其中。");
        Add(74, "野性之杖", "狂野而不可预测，就像娜琪亚本人。精灵们喜爱它的混乱能量。");
        Add(75, "守护者法杖", "由保护森林者携带。娜琪亚生来就继承了这个责任。");
        Add(76, "灵界魔杖", "连接物质世界和精灵世界的魔杖。娜琪亚行走于两者之间。");
        Add(77, "霜木法杖", "在永恒冬季中冻结的法杖。北方的寒冷精灵听从它的召唤。");
        Add(78, "暗影之根法杖", "在森林最深处生长。暗影精灵围绕它舞蹈。");
        Add(79, "余烬之木法杖", "永不停止燃烧的法杖。火焰精灵被它的温暖吸引。");
        Add(80, "阳光祝福法杖", "被黎明精灵祝福。它的光芒引导迷失的灵魂回家。");
        Add(81, "芬里尔之牙", "顶端镶嵌精灵狼牙的法杖。娜琪亚忠诚的伙伴引导它的力量。");
        Add(82, "古橡法杖", "由千年橡树雕刻。最古老的精灵还记得它被种下的时候。");
        Add(83, "世界树之枝", "来自传说中世界树的树枝。所有森林精灵都向它鞠躬。");
        Add(84, "精灵王之杖", "精灵王本人的法杖。娜琪亚通过保护森林赢得了它。");
        Add(85, "永冻法杖", "来自森林冰冻之心的永恒之冰法杖。冬季精灵服从它的命令。");
        Add(86, "凤凰栖木", "火焰精灵栖息的法杖。它的火焰带来重生，而非毁灭。");
        Add(87, "光辉林地法杖", "散发千只萤火虫光芒的法杖。希望精灵在其中舞蹈。");
        Add(88, "虚空守望者法杖", "守护世界边界的法杖。暗影精灵保护它的持有者。");
        Add(89, "新手刀", "学习刀道者的基础武士刀。空壳在几天内就精通了它。");
        Add(90, "寂静刀", "设计成不发出声音的武士刀。空壳的受害者永远听不到死亡来临。");
        Add(91, "刺客刀", "细长而精准。空壳只需一击正确的位置。");
        Add(92, "扑杀刀", "高效。实用。无情。就像空壳本身。");
        Add(93, "追猎刀", "用于狩猎的武士刀。空壳永不停止直到目标被消灭。");
        Add(94, "空心刀", "以空壳感受到的空虚命名。或者说，感受不到的。");
        Add(95, "处刑者之刀", "终结无数生命的武士刀。空壳对他们任何一个都没有感觉。");
        Add(96, "精准之刀", "调谐来利用弱点的武士刀。空壳计算最佳攻击。");
        Add(97, "霜咬刀", "冰冷恶意的武士刀。它的寒冷与空壳空洞的心相配。");
        Add(98, "净化刀", "用于黑暗工作的光明武士刀。空壳没有感受到讽刺。");
        Add(99, "暗影刀", "饮入阴影的武士刀。空壳无形地移动，无声地出击。");
        Add(100, "灼热匕首", "被内火加热的武士刀。空壳的目标在流血前先被烧灼。");
        Add(101, "完美凶器", "终极杀手的终极工具。空壳为此刀而生。");
        Add(102, "无情之刃", "永不钝化的武士刀，由永不停止的追击者挥舞。");
        Add(103, "地狱刺客", "在地狱火中锻造的武士刀。它燃烧着空壳单一目的的强度。");
        Add(104, "虚空刀", "绝对黑暗的武士刀。像空壳一样，它只为终结而存在。");
        Add(105, "冰封刀", "永恒之冰的武士刀。它的裁决如空壳一样冰冷而终结。");
        Add(106, "光辉终结", "光明武士刀的终极形态。空壳以艾尔的名义执行审判。");
        Add(107, "新手权杖", "太阳烈焰骑士团新入者的简单权杖。维斯珀从这里开始他的旅程。");
        Add(108, "审判之锤", "用于执行艾尔旨意的战锤。每一击都是神圣的裁决。");
        Add(109, "火焰守护者之棍", "守护圣火者携带的武器。它的打击净化不洁。");
        Add(110, "侍僧之盾", "骑士团侍僧的简单盾牌。信仰是最好的防御。");
        Add(111, "审判官之盾", "审判官携带的盾牌。它见证了无数异端的陨落。");
        Add(112, "黄昏防御者", "在光明与黑暗之间守护的盾牌。");
        Add(113, "狂热者粉碎锤", "燃烧着宗教狂热的战锤。敌人在其面前颤抖。");
        Add(114, "净化者重锤", "用于净化邪恶的巨大战锤。一击粉碎罪恶。");
        Add(115, "艾尔的裁决", "被艾尔亲自祝福的战锤。它的判决不可上诉。");
        Add(116, "信仰壁垒", "坚不可摧的盾牌。只要信仰不灭，它就不会破碎。");
        Add(117, "圣焰神盾", "被圣火环绕的盾牌。触碰它的邪恶会被焚烧。");
        Add(118, "宗教裁判壁垒", "高级审判官的盾牌。维斯珀通过无情的服务赢得了它。");
        Add(119, "破晓者", "粉碎黑暗的传奇战锤。维斯珀挥舞着艾尔愤怒的化身。");
        Add(120, "神圣毁灭者", "被最高祭司祝福的战锤。它的打击承载着神圣愤怒的重量。");
        Add(121, "艾尔的右手", "教团最神圣的武器。维斯珀是艾尔旨意的选定工具。");
        Add(122, "圣焰堡垒", "教团的终极防御。艾尔的光芒保护维斯珀免受一切伤害。");
        Add(123, "永恒警戒", "永不动摇的盾牌。像维斯珀一样，它永远守望黑暗。");
        Add(124, "黄昏圣骑士之誓", "教团领袖的盾牌。维斯珀发誓将灵魂绑定于艾尔。");
        Add(125, "初生之星", "一颗刚刚形成的新星。尤巴还记得所有星星都这么小的时候。");
        Add(126, "宇宙余烬", "恒星之火的碎片。它燃烧着创世本身的热量。");
        Add(127, "星尘球", "等待被塑造的压缩星尘。尤巴用这样的物质编织星系。");
        Add(128, "梦境催化剂", "放大梦境能量的球体。源自尤巴最初的实验。");
        Add(129, "星云核心", "遥远星云的心脏。尤巴从宇宙织锦中摘取了它。");
        Add(130, "天体种子", "终将成为恒星的种子。尤巴培育着无数这样的种子。");
        Add(131, "超新星碎片", "爆炸恒星的碎片。尤巴见证了它的死亡并保存了它的力量。");
        Add(132, "引力井", "压缩引力的球体。空间本身在它周围弯曲。");
        Add(133, "虚空奇点", "无限密度的点。尤巴用它创造和毁灭世界。");
        Add(134, "日冕", "太阳外层大气的碎片。它燃烧着恒星的愤怒。");
        Add(135, "冰冻彗星", "被捕获的彗星核心。它携带着宇宙深处的秘密。");
        Add(136, "恒星熔炉", "恒星核心的缩影。尤巴用它锻造新的现实。");
        Add(137, "大爆炸遗物", "创世大爆炸的碎片。它包含着宇宙的起源。");
        Add(138, "宇宙织机", "编织现实的工具。尤巴用它重塑命运。");
        Add(139, "光辉创世", "创世之光本身。尤巴用它诞生了第一批恒星。");
        Add(140, "熵之心", "宇宙衰败的精华。万物终结，而尤巴决定何时。");
        Add(141, "绝对零度", "存在中最冷的点。即使时间也在它面前冻结。");
        Add(142, "灾变核心", "宇宙毁灭的心脏。尤巴只在危急时刻才释放它。");
        Add(143, "梦者之环", "初入梦境世界者佩戴的简单戒指。它散发着微弱的希望。");
        Add(144, "星尘之环", "由凝聚的星尘锻造。梦者们说它在危险时刻会更亮。");
        Add(145, "沉睡印记", "刻有安眠标记。佩戴者能更快从伤口中恢复。");
        Add(146, "精灵之环", "捕捉游荡精灵的戒指。它们的光芒指引穿越黑暗梦境的道路。");
        Add(147, "流浪者之环", "梦境领域的流浪者佩戴。它见证了无数被遗忘的道路。");
        Add(148, "噩梦碎片戒指", "包含被击败噩梦碎片的戒指。它的黑暗赋予勇者力量。");
        Add(149, "记忆守护者", "保存珍贵记忆的戒指。斯穆迪在他的商店出售类似的。");
        Add(150, "月华奥术之环", "来自月华奥术会的戒指。奥蕾娜在流放前曾戴过类似的。");
        Add(151, "灵界森林印记", "被娜琪亚森林的精灵祝福的戒指。自然之力流经其中。");
        Add(152, "皇家卫队徽章", "拉塞尔塔服役时佩戴的戒指。它仍承载着责任的重量。");
        Add(153, "决斗者荣耀", "在贵族决斗者中传承的戒指。米斯特的家族珍视这样的戒指。");
        Add(154, "圣焰余烬护符", "被封存的圣焰火花。维斯珀的教团守护着这些圣物。");
        Add(155, "法术书碎片", "由铋晶结晶魔力制成的戒指。它嗡嗡作响着奥术能量。");
        Add(156, "追猎者印记", "追踪猎物的戒指。空壳的操控者用这些来监视他们的工具。");
        Add(157, "艾尔的祝福", "被艾尔本身祝福的戒指。它的光芒驱散噩梦并治愈伤者。");
        Add(158, "噩梦领主印记", "从被击败的噩梦领主那里取得。它黑暗的力量腐化并赋能。");
        Add(159, "阿斯特里德遗产", "来自阿斯特里德贵族之家的戒指。米斯特的祖先戴着它参加无数决斗。");
        Add(160, "梦见者之眼", "能看穿梦境面纱的戒指。过去、现在和未来模糊在一起。");
        Add(161, "原初梦境之环", "在梦境黎明锻造。它包含第一个梦者的精华。");
        Add(162, "永恒沉睡", "触及最深沉睡的戒指。佩戴者既不惧死亡也不惧醒来。");
        Add(163, "梦者吊坠", "初入梦境世界者佩戴的简单吊坠。它散发着微弱的希望。");
        Add(164, "火焰之触挂坠", "被火焰触碰的挂坠。它的温暖驱散寒冷和恐惧。");
        Add(165, "猩红泪滴", "形如血泪的红宝石。它的力量来自牺牲。");
        Add(166, "琥珀护符", "包含远古生物的琥珀。它的记忆赋予佩戴者力量。");
        Add(167, "黄昏奖章", "在光明与黑暗间闪烁的奖章。维斯珀的新入者佩戴这些。");
        Add(168, "星尘项链", "撒满结晶星星的项链。斯穆迪从坠落的梦境中收集这些。");
        Add(169, "翡翠之眼", "能看穿幻象的绿色宝石。噩梦无法躲避它的凝视。");
        Add(170, "森林精灵之心", "包含森林精灵祝福的宝石。娜琪亚的守护者珍视这些。");
        Add(171, "月华奥术印记", "来自禁忌档案的印记。奥蕾娜冒一切风险研究这些。");
        Add(172, "铋晶水晶", "纯净魔力的结晶碎片。它与奥术能量共鸣。");
        Add(173, "圣焰余烬护符", "被封存的圣焰火花。维斯珀的教团守护着这些圣物。");
        Add(174, "噩梦之牙吊坠", "来自强大噩梦的獠牙，现在是战利品。空壳戴着一个作为提醒。");
        Add(175, "皇家卫队奖章", "来自皇家卫队的荣誉奖章。拉塞尔塔在流放前获得了许多。");
        Add(176, "阿斯特里德纹章", "阿斯特里德家族的贵族纹章。米斯特的家族遗产挂在这条链子上。");
        Add(177, "艾尔神圣之泪", "艾尔本身流下的泪滴。它的光芒保护免受最黑暗噩梦的侵害。");
        Add(178, "原初翡翠", "来自第一片森林的宝石。它包含古老精灵的梦境。");
        Add(179, "噩梦领主之眼", "被击败噩梦领主的眼睛。它看到所有恐惧并利用它们。");
        Add(180, "圣焰之心", "圣焰本身的核心。只有最虔诚者才能佩戴它。");
        Add(181, "梦见者神谕", "能窥见所有可能未来的护身符。现实向它的预言弯曲。");
        Add(182, "虚空行者罗盘", "指向虚空的护身符。跟随它的人永远不会以同样的方式返回。");
        Add(183, "迈达斯护符", "被黄金之梦触碰的传奇护符。被击杀的敌人会掉落额外金币。");
        Add(478, "星尘收集者", "收集来自被击败敌人结晶星尘的传奇护符。每次击杀都会产生珍贵的梦境精华（基础1-5，随强化等级和怪物强度提升）。");
        Add(184, "流浪者腰带", "在梦境间漂泊者佩戴的简单布腰带。它将灵魂锚定。");
        Add(185, "梦者之绳", "由梦丝编织的绳带。它随每次心跳轻轻跳动。");
        Add(186, "星尘扣带", "装饰着结晶星尘的腰带。斯穆迪出售类似的饰品。");
        Add(187, "哨兵腰带", "梦境守望者佩戴的坚固腰带。它永不松动，永不失效。");
        Add(188, "神秘束带", "注入微弱魔力的腰带。当噩梦靠近时它会刺痛。");
        Add(189, "猎人皮带", "带有补给袋的皮革腰带。在梦境中长途旅行的必需品。");
        Add(190, "朝圣者腰带", "寻求艾尔之光者佩戴。它在最黑暗的梦境中提供慰藉。");
        Add(191, "噩梦猎人腰带", "由被击败的噩梦制成的腰带。它们的精华强化佩戴者。");
        Add(192, "奥术腰带", "来自月华奥术会的腰带。它放大魔法能量。");
        Add(193, "圣焰束带", "被维斯珀教团祝福的腰带。它燃烧着保护之火。");
        Add(194, "决斗者剑带", "阿斯特里德贵族战士佩戴的精美腰带。它承载刀剑和荣誉。");
        Add(195, "精灵编织者之绳", "森林精灵为娜琪亚编织的腰带。它嗡嗡作响着自然能量。");
        Add(196, "刺客工具带", "带有隐藏隔层的腰带。空壳的操控者用这些装备他们的工具。");
        Add(197, "枪手枪套", "为拉塞尔塔的火器设计的腰带。快速拔枪，更快击杀。");
        Add(198, "艾尔神圣腰带", "被艾尔本身祝福的腰带。它的光芒将灵魂锚定对抗一切黑暗。");
        Add(199, "噩梦君主之链", "由噩梦领主锁链锻造的腰带。它将恐惧绑定于佩戴者的意志。");
        Add(200, "原初梦境腰带", "来自第一个梦境的腰带。它包含创世本身的回声。");
        Add(201, "世界树根腰带", "由世界树根生长的腰带。娜琪亚的森林祝福了它的创造。");
        Add(202, "阿斯特里德传家宝", "阿斯特里德家族的祖传腰带。米斯特的血脉编织在它的织物中。");
        Add(203, "黄昏审判官束带", "维斯珀最高级别的腰带。它审判站在它面前的所有人。");
        Add(204, "骸骨哨兵面具", "由堕落噩梦的头骨雕刻的面具。它空洞的眼睛能看穿幻象。");
        Add(205, "灵界守护者头盔", "注入娜琪亚森林精灵的头盔。保护的低语环绕着佩戴者。");
        Add(206, "噩梦君主之冠", "由被征服的噩梦锻造的王冠。佩戴者支配恐惧本身。");
        Add(207, "骸骨哨兵背心", "来自死于梦中的生物的肋骨护甲。它们的保护延续至死后。");
        Add(208, "灵界守护者胸甲", "由凝固的梦境编织的胸甲。它会移动和适应来袭的打击。");
        Add(209, "噩梦君主胸甲", "随黑暗能量跳动的护甲。噩梦在其佩戴者面前鞠躬。");
        Add(210, "骸骨哨兵护胫", "用噩梦骨骼加固的腿甲。它们永不疲倦，永不动摇。");
        Add(211, "灵界守护者腿甲", "被森林精灵祝福的腿甲。佩戴者如风穿过树叶般移动。");
        Add(212, "噩梦君主腿甲", "饮入阴影的腿甲。黑暗赋予每一步力量。");
        Add(213, "骸骨哨兵踏靴", "在梦境领域中无声行走的靴子。死者不发出声音。");
        Add(214, "灵界守护者铁靴", "在梦境领域中不留痕迹的靴子。非常适合狩猎噩梦者。");
        Add(215, "噩梦君主之靴", "跨越世界的靴子。现实在每一步下弯曲。");
        Add(216, "奥蕾娜的学徒帽", "月光奥术新手佩戴的简单帽子。");
        Add(217, "奥蕾娜的神秘头巾", "注入微弱月光能量的头巾。");
        Add(218, "奥蕾娜的魅惑冠冕", "增强佩戴者魔法亲和力的冠冕。");
        Add(219, "奥蕾娜的古老冠冕", "代代相传的古老神器。");
        Add(220, "奥蕾娜的猎鹰头环", "受天空精灵祝福的头环。");
        Add(221, "奥蕾娜的力量冠冕", "散发奥术力量的强大冠冕。");
        Add(222, "奥蕾娜的月光王冠", "月光奥术大师的传奇王冠。");
        Add(223, "奥蕾娜的蓝袍", "月光法师穿的简单长袍。");
        Add(224, "奥蕾娜的魔法师袍", "修炼法师喜爱的长袍。");
        Add(225, "奥蕾娜的织咒袍", "用魔法丝线编织的长袍。");
        Add(226, "奥蕾娜的启迪袍", "引导月光智慧的长袍。");
        Add(227, "奥蕾娜的梦境裹布", "由凝固的梦境编织的裹布。");
        Add(228, "奥蕾娜的皇家鳞袍", "适合月光皇族的长袍。");
        Add(229, "奥蕾娜的冰雪女王袍", "冰雪女王本人的传奇长袍。");
        Add(230, "奥蕾娜的蓝色护腿", "有抱负的法师穿的简单护腿。");
        Add(231, "奥蕾娜的纤维裙", "允许自由移动的轻便裙子。");
        Add(232, "奥蕾娜的精灵护腿", "精灵设计的优雅护腿。");
        Add(233, "奥蕾娜的启迪护腿", "受智慧祝福的护腿。");
        Add(234, "奥蕾娜的冰川短裙", "注入冰霜魔法的短裙。");
        Add(235, "奥蕾娜的冰霜裙裤", "散发寒冷力量的裙裤。");
        Add(236, "奥蕾娜的华丽护腿", "无与伦比优雅的传奇护腿。");
        Add(237, "柔软月光拖鞋", "月光奥术侍僧穿的柔软鞋子。它们用月光缓冲每一步。");
        Add(238, "朝圣者凉鞋", "被会社长老祝福的简单凉鞋。奥蕾娜仍记得它们的温暖。");
        Add(239, "东方神秘之鞋", "来自遥远国度的异域鞋履。它们通过大地引导治愈能量。");
        Add(240, "梦行者之恩", "在清醒与梦境世界间行走的靴子。非常适合寻找迷失灵魂的治愈者。");
        Add(241, "灵魂行者之履", "注入已故治愈者精华的靴子。他们的智慧引导每一步。");
        Add(242, "灵魂追踪者之靴", "追踪伤者的黑暗靴子。奥蕾娜用它们找到需要帮助的人...或那些伤害过她的人。");
        Add(243, "堕落贤者之翼", "据说能赐予纯洁之心飞行能力的传奇靴子。奥蕾娜的流放证明了她的资格。");
        Add(244, "铋晶的锥形帽", "水晶学徒的简单帽子。");
        Add(245, "铋晶的翡翠帽", "镶嵌翡翠水晶的帽子。");
        Add(246, "铋晶的冰川面具", "冰冻水晶制成的面具。");
        Add(247, "铋晶的亚拉哈里面具", "古老的水晶力量面具。");
        Add(248, "铋晶的棱镜头盔", "将光线折射成彩虹的头盔。");
        Add(249, "铋晶的反射王冠", "反射魔法能量的王冠。");
        Add(250, "铋晶的白炽王冠", "燃烧着水晶之火的传奇王冠。");
        Add(251, "铋晶的冰袍", "被水晶魔法冷却的长袍。");
        Add(252, "铋晶的僧侣袍", "用于冥想的简单长袍。");
        Add(253, "铋晶的冰川袍", "冻结在永恒冰霜中的长袍。");
        Add(254, "铋晶的水晶铠甲", "由活水晶形成的铠甲。");
        Add(255, "铋晶的棱镜铠甲", "弯曲光线本身的铠甲。");
        Add(256, "铋晶的冰冻板甲", "永恒霜冻的板甲。");
        Add(257, "铋晶的神圣板甲", "受水晶神祝福的传奇铠甲。");
        Add(258, "铋晶的链甲护腿", "带水晶链环的基本护腿。");
        Add(259, "铋晶的纤维护腿", "水晶法师的轻便护腿。");
        Add(260, "铋晶的翡翠护腿", "装饰着翡翠水晶的护腿。");
        Add(261, "铋晶的奇异马裤", "脉动着奇异能量的马裤。");
        Add(262, "铋晶的棱镜护腿", "闪烁着棱镜光芒的护腿。");
        Add(263, "铋晶的亚拉哈里腿甲", "亚拉哈里的古老护腿。");
        Add(264, "铋晶的猎鹰护胫", "猎鹰骑士团的传奇护胫。");
        Add(265, "学者拖鞋", "在图书馆长时间工作的舒适鞋履。知识流淌于其鞋底。");
        Add(266, "亚拉哈里裹足", "来自失落文明的古老裹布。它们低语着被遗忘的咒语。");
        Add(267, "冰川之鞋", "由永恒之冰雕刻的靴子。铋晶行走之处留下霜冻图案。");
        Add(268, "霜花之步", "绽放冰晶的精致靴子。每一步都创造一座霜冻花园。");
        Add(269, "水晶共鸣之靴", "放大魔法能量的靴子。水晶嗡嗡作响着未开发的力量。");
        Add(270, "棱镜奥术之靴", "将光线折射成纯粹魔法能量的靴子。现实在每一步周围弯曲。");
        Add(271, "虚空行者之靴", "穿越虚空本身的传奇靴子。铋晶能看到他人无法想象的道路。");
        Add(272, "蜥蜴人的皮革头盔", "龙战士的基本头盔。");
        Add(273, "蜥蜴人的链甲头盔", "坚固的链甲头盔。");
        Add(274, "蜥蜴人的战士头盔", "经验丰富的战士佩戴的头盔。");
        Add(275, "蜥蜴人的龙鳞头盔", "由龙鳞锻造的头盔。");
        Add(276, "蜥蜴人的皇家头盔", "适合龙族皇室的头盔。");
        Add(277, "蜥蜴人的精英龙人头盔", "龙人骑士团的精英头盔。");
        Add(278, "蜥蜴人的黄金头盔", "龙王的传奇黄金头盔。");
        Add(279, "蜥蜴人的皮革铠甲", "龙战士的基本铠甲。");
        Add(280, "蜥蜴人的鳞甲", "用鳞片加固的铠甲。");
        Add(281, "蜥蜴人的骑士铠甲", "龙骑士的重型铠甲。");
        Add(282, "蜥蜴人的龙鳞锁甲", "由龙鳞锻造的锁甲。");
        Add(283, "蜥蜴人的皇家龙人锁甲", "龙人骑士团的皇家锁甲。");
        Add(284, "蜥蜴人的猎鹰板甲", "猎鹰骑士团的板甲。");
        Add(285, "蜥蜴人的黄金铠甲", "龙王的传奇黄金铠甲。");
        Add(286, "蜥蜴人的皮革护腿", "龙战士的基本护腿。");
        Add(287, "蜥蜴人的铆钉护腿", "用铆钉加固的护腿。");
        Add(288, "蜥蜴人的骑士护腿", "龙骑士的重型护腿。");
        Add(289, "蜥蜴人的龙鳞护腿", "由龙鳞锻造的护腿。");
        Add(290, "蜥蜴人的王冠护腿", "适合皇室的护腿。");
        Add(291, "蜥蜴人的华丽护腿", "大师工艺的华丽护腿。");
        Add(292, "蜥蜴人的黄金护腿", "龙王的传奇黄金护腿。");
        Add(293, "磨损皮靴", "经历过许多战斗的简单靴子。它们带着冒险的气息。");
        Add(294, "补丁战靴", "无数次修补的靴子。每个补丁都诉说着生存的故事。");
        Add(295, "钢铁战士之靴", "为战斗锻造的重型靴子。它们在战斗中将拉塞尔塔锚定于地面。");
        Add(296, "守护者护胫", "精英保护者穿戴的靴子。它们永不后退，永不投降。");
        Add(297, "龙鳞护胫", "由龙鳞制作的靴子。它们赋予佩戴者龙族的坚韧。");
        Add(298, "龙人军阀之靴", "古代龙人将军的靴子。它们在任何战场上都令人敬畏。");
        Add(299, "黄金冠军之靴", "一个时代最伟大战士穿戴的传奇靴子。拉塞尔塔的命运在等待。");
        Add(300, "迷雾的头巾", "简单的头巾，隐藏身份。");
        Add(301, "迷雾的轻便头巾", "轻便透气的头巾。");
        Add(302, "迷雾的暗视头巾", "增强夜视的头巾。");
        Add(303, "迷雾的闪电头带", "充满电能的头带。");
        Add(304, "迷雾的眼镜蛇兜帽", "如蛇般致命的兜帽。");
        Add(305, "迷雾的地狱追踪者面罩", "追踪猎物的面罩。");
        Add(306, "迷雾的黑暗低语", "低语死亡的传奇面具。");
        Add(307, "迷雾的皮革挽具", "用于敏捷的轻便挽具。");
        Add(308, "迷雾的游侠斗篷", "用于快速移动的斗篷。");
        Add(309, "迷雾的午夜束腰外衣", "用于夜间打击的束腰外衣。");
        Add(310, "迷雾的闪电袍", "充满闪电的长袍。");
        Add(311, "迷雾的电压铠甲", "涌动着电力的铠甲。");
        Add(312, "迷雾的大师弓箭手铠甲", "弓箭大师的铠甲。");
        Add(313, "迷雾的黑暗领主斗篷", "黑暗领主的传奇斗篷。");
        Add(314, "迷雾的游侠护腿", "游侠穿的护腿。");
        Add(315, "迷雾的丛林生存者护腿", "丛林生存者的护腿。");
        Add(316, "迷雾的午夜纱笼", "午夜行动的纱笼。");
        Add(317, "迷雾的闪电护腿", "充满闪电的护腿。");
        Add(318, "迷雾的蚱蜢护腿", "用于惊人跳跃的护腿。");
        Add(319, "迷雾的绿魔护腿", "恶魔速度的护腿。");
        Add(320, "迷雾的灵魂漫步者护腿", "在世界间跨步的传奇护腿。");
        Add(321, "临时侦察靴", "用碎片拼凑的靴子。轻便安静，适合侦察。");
        Add(322, "沼泽奔跑者之靴", "穿越困难地形的防水靴子。迷雾知道每一条捷径。");
        Add(323, "疾速行者", "加速佩戴者步伐的附魔靴子。非常适合打了就跑的战术。");
        Add(324, "雄鹿猎人之靴", "追踪猎物的靴子。");
        Add(325, "闪电打击者之靴", "充满闪电能量的靴子。");
        Add(326, "火行者战靴", "能在火焰中行走的靴子。");
        Add(327, "苦难践踏者", "践踏一切苦难的传奇靴子。");
        Add(328, "娜琪亚的毛皮帽", "森林猎人的简单帽子。");
        Add(329, "娜琪亚的羽毛头饰", "装饰着羽毛的头饰。");
        Add(330, "娜琪亚的萨满面具", "精神力量的面具。");
        Add(331, "娜琪亚的大地兜帽", "受大地精灵祝福的兜帽。");
        Add(332, "娜琪亚的自然头盔", "注入自然力量的头盔。");
        Add(333, "娜琪亚的树木王冠", "由古树生长而成的王冠。");
        Add(334, "娜琪亚的树叶王冠", "森林守护者的传奇王冠。");
        Add(335, "娜琪亚的毛皮铠甲", "由森林生物制成的铠甲。");
        Add(336, "娜琪亚的原住民铠甲", "森林部落的传统铠甲。");
        Add(337, "娜琪亚的绿木外套", "由活藤蔓编织的外套。");
        Add(338, "娜琪亚的大地披风", "受大地精灵祝福的披风。");
        Add(339, "娜琪亚的自然拥抱", "与自然合一的铠甲。");
        Add(340, "娜琪亚的沼泽巢穴铠甲", "来自深沼的铠甲。");
        Add(341, "娜琪亚的树叶袍", "森林守护者的传奇长袍。");
        Add(342, "娜琪亚的猛犸毛皮短裤", "由猛犸毛皮制成的温暖短裤。");
        Add(343, "娜琪亚的皮短裤", "传统狩猎护腿。");
        Add(344, "娜琪亚的雄鹿护腿", "由雄鹿皮制成的护腿。");
        Add(345, "娜琪亚的大地护腿", "受大地精灵祝福的护腿。");
        Add(346, "娜琪亚的树叶护腿", "由魔法树叶编织的护腿。");
        Add(347, "娜琪亚的野猪人腰布", "来自强大野猪人的腰布。");
        Add(348, "娜琪亚的血色护胫", "血猎的传奇护胫。");
        Add(349, "毛皮内衬靴", "温暖舒适的靴子。");
        Add(350, "獾皮靴", "由獾皮制成的靴子。");
        Add(351, "眼镜蛇打击靴", "如蛇般迅速的靴子。");
        Add(352, "森林潜行者裹布", "在森林中悄无声息的裹布。");
        Add(353, "大地猎人之靴", "追踪猎物的靴子。");
        Add(354, "热病花游侠之靴", "装饰着热病花的靴子。它们赋予狂热的速度和致命的精准。");
        Add(355, "血色掠食者之靴", "沾满无数狩猎鲜血的传奇靴子。娜琪亚成为顶级掠食者。");
        Add(356, "空壳的破损头盔", "来自地下世界的破旧头盔。");
        Add(357, "空壳的破碎面罩", "仍能提供保护的破裂面罩。");
        Add(358, "空壳的骨领主头盔", "由骨领主遗骸锻造的头盔。");
        Add(359, "空壳的骷髅头盔", "由骷髅塑造的头盔。");
        Add(360, "空壳的死亡头盔", "死亡本身的头盔。");
        Add(361, "空壳的诺克费拉图骷髅护卫", "吸血鬼骨骼的头盔。");
        Add(362, "空壳的恶魔头盔", "恶魔领主的传奇头盔。");
        Add(363, "空壳的葬礼裹布", "来自坟墓的裹布。");
        Add(364, "空壳的旧斗篷", "来自远古时代的破旧斗篷。");
        Add(365, "空壳的诺克费拉图骨斗篷", "由骨骼编织的斗篷。");
        Add(366, "空壳的灵魂斗篷", "不安灵魂的斗篷。");
        Add(367, "空壳的死亡长袍", "死亡的长袍。");
        Add(368, "空壳的地下世界长袍", "来自深渊的长袍。");
        Add(369, "空壳的恶魔铠甲", "恶魔领主的传奇铠甲。");
        Add(370, "空壳的破损裙甲", "破损但仍有用的裙甲。");
        Add(371, "空壳的变异骨裙", "由变异骨骼制成的裙子。");
        Add(372, "空壳的诺克费拉图荆棘裹布", "带刺的裹布。");
        Add(373, "空壳的诺克费拉图血肉护卫", "由保存的血肉制成的护具。");
        Add(374, "空壳的血色护腿", "血染的护腿。");
        Add(375, "空壳的诺克费拉图血行者", "在血液中行走的护腿。");
        Add(376, "空壳的恶魔护腿", "恶魔领主的传奇护腿。");
        Add(377, "铁甲护踝", "踩碎脚下一切的重型金属靴子。防守就是最好的进攻。");
        Add(378, "航海者之靴", "能抵御任何风暴的坚固靴子。它们将空壳锚定于战斗的潮流中。");
        Add(379, "深渊护胫", "在海洋深处锻造的靴子。它们能抵抗一切压力。");
        Add(380, "碎骨之靴", "用怪物骨骼加固的靴子。每一步都是威胁。");
        Add(381, "岩浆堡垒之靴", "在火山之火中锻造的靴子。它们散发的热量能灼烧攻击者。");
        Add(382, "血踏者之靴", "留下毁灭痕迹的残暴靴子。敌人害怕与之交战。");
        Add(383, "坚定守护者之靴", "不可摧毁防御者的传奇靴子。空壳成为不可移动的堡垒。");
        Add(384, "维斯珀的女巫帽", "织梦者的简单帽子。");
        Add(385, "维斯珀的诡异兜帽", "低语秘密的兜帽。");
        Add(386, "维斯珀的奇异兜帽", "脉动着奇异能量的兜帽。");
        Add(387, "维斯珀的奇异风帽", "噩梦能量的风帽。");
        Add(388, "维斯珀的绝望裹布", "以绝望为食的裹布。");
        Add(389, "维斯珀的黑暗巫师王冠", "黑暗魔法的王冠。");
        Add(390, "维斯珀的费伦布拉斯帽", "黑暗大师的传奇帽子。");
        Add(391, "维斯珀的红袍", "有抱负的梦法师的长袍。");
        Add(392, "维斯珀的灵魂束缚", "引导灵魂的束缚。");
        Add(393, "维斯珀的能量袍", "噼啪作响的能量长袍。");
        Add(394, "维斯珀的幽灵裙", "由幽灵能量编织的裙子。");
        Add(395, "维斯珀的灵魂披风", "容纳灵魂的披风。");
        Add(396, "维斯珀的灵魂裹布", "被捕获灵魂的裹布。");
        Add(397, "维斯珀的奥术龙袍", "奥术龙的传奇长袍。");
        Add(398, "维斯珀的灵魂护腿", "注入灵魂能量的护腿。");
        Add(399, "维斯珀的异域护腿", "来自遥远领域的异域护腿。");
        Add(400, "维斯珀的灵魂腿甲", "引导灵魂力量的护腿。");
        Add(401, "维斯珀的血色裤子", "被血魔法染色的裤子。");
        Add(402, "维斯珀的岩浆护腿", "在魔法火焰中锻造的护腿。");
        Add(403, "维斯珀的智慧护腿", "古老智慧的护腿。");
        Add(404, "维斯珀的古人裤子", "来自古人的传奇裤子。");
        Add(405, "侍僧拖鞋", "神殿新入者穿的柔软拖鞋。它们在每一步中承载祈祷。");
        Add(406, "神殿之鞋", "艾尔仆人的传统鞋履。它们将维斯珀锚定于她的信仰。");
        Add(407, "奇异僧侣之靴", "研究禁忌典籍的僧侣穿的靴子。知识与信仰交织。");
        Add(408, "侏儒祝福裹足", "被侏儒工匠附魔的裹足。科技与神性相遇。");
        Add(409, "吸血鬼丝绸拖鞋", "用吸血鬼丝绸编织的优雅拖鞋。它们从大地本身汲取生命。");
        Add(410, "恶魔杀手拖鞋", "被祝福以摧毁邪恶的绿色拖鞋。它们在每一步中灼烧恶魔。");
        Add(411, "噩梦驱逐者之靴", "穿越噩梦拯救无辜者的传奇靴子。维斯珀的终极使命。");
        Add(412, "尤巴的部落面具", "部落战士的面具。");
        Add(413, "尤巴的维京头盔", "北方战士的头盔。");
        Add(414, "尤巴的角盔", "带有可怕角的头盔。");
        Add(415, "尤巴的坚忍伊克斯头饰", "伊克斯部落的头饰。");
        Add(416, "尤巴的恐火头饰", "燃烧着恐惧的头饰。");
        Add(417, "尤巴的坚忍伊克斯头盔", "伊克斯勇士的头盔。");
        Add(418, "尤巴的末日面容", "天启的传奇面容。");
        Add(419, "尤巴的熊皮", "来自强大熊的铠甲。");
        Add(420, "尤巴的猛犸毛皮斗篷", "来自猛犸的斗篷。");
        Add(421, "尤巴的坚忍伊克斯袍", "伊克斯部落的长袍。");
        Add(422, "尤巴的岩浆外套", "在岩浆中锻造的外套。");
        Add(423, "尤巴的坚忍伊克斯胸甲", "伊克斯勇士的胸甲。");
        Add(424, "尤巴的熔岩板甲", "熔岩的板甲。");
        Add(425, "尤巴的火生巨人铠甲", "火巨人的传奇铠甲。");
        Add(426, "尤巴的坚忍伊克斯腰甲", "伊克斯部落的腿甲。");
        Add(427, "尤巴的变异皮裤", "由变异皮革制成的裤子。");
        Add(428, "尤巴的坚忍伊克斯腿裙", "伊克斯战士的腿裙。");
        Add(429, "尤巴的矮人护腿", "坚固的矮人护腿。");
        Add(430, "尤巴的合金护腿", "加固合金的护腿。");
        Add(431, "尤巴的板甲护腿", "重型板甲护腿。");
        Add(432, "尤巴的侏儒护腿", "侏儒工艺的传奇护腿。");
        Add(433, "临时战靴", "用战场碎片组装的靴子。尤巴用他能找到的东西凑合。");
        Add(434, "部落裹足", "尤巴族人的传统裹布。它们将他与祖先连接。");
        Add(435, "雄鹿战士护胫", "装饰着雄鹿角的靴子。它们赋予森林之王的力量。");
        Add(436, "獠牙践踏者之靴", "带有獠牙状尖刺的残暴靴子。它们将敌人踩在脚下。");
        Add(437, "血色狂战士之靴", "血红色的靴子，激发尤巴的狂怒。痛苦变成力量。");
        Add(438, "灵魂践踏者", "收割倒下敌人灵魂的靴子。它们的力量随每场战斗增长。");
        Add(439, "归乡冠军之靴", "在精神上将尤巴带回家乡的传奇靴子。他的祖先与他并肩作战。");
        Add(440, "梦行者的飞翼头盔", "赐予梦行者飞行力量的传奇头盔。自动攻击最近的敌人。");
        Add(441, "梦境魔法板甲", "保护所有领域梦行者的传奇铠甲。自动瞄准最近的敌人。");
        Add(442, "深渊护胫", "在梦境最深处锻造的传奇护腿。");
        Add(443, "水上行走之靴", "让佩戴者能在水上行走的传奇靴子。所有领域的梦行者都在寻找的宝藏。");
        Add(444, "梦行者之剂", "用现实世界中发现的草药酿造的简单药剂。即使是最微弱的梦也始于一滴希望。");
        Add(445, "幻梦精华", "从平静睡眠的记忆中蒸馏而成。饮用者会感到被遗忘的梦的温暖洗涤他们的伤口。");
        Add(446, "清醒活力", "由斯穆迪使用星尘和噩梦碎片制作。液体闪烁着千颗沉睡之星的光芒。");
        Add(447, "艾尔圣药", "受艾尔之光祝福的神圣药剂。维斯珀的骑士团严守其配方，因为它能治愈最黑暗噩梦造成的伤口。");
        
        // Universal Legendary Belt (448)
        Add(448, "无限梦境腰带", "将佩戴者与永恒梦境循环绑定的传奇腰带。每征服一个噩梦，它的力量就会增长。");
        
        // Hero-Specific Legendary Belts (449-457)
        Add(449, "奥蕾娜的生命编织者腰带", "由奥蕾娜被窃取的生命力研究锻造的腰带。它将生与死编织成完美的平衡。");
        Add(450, "铋晶的水晶束带", "纯净水晶制成的腰带，随着盲女视野的脉动。它能看到眼睛无法看到的东西。");
        Add(451, "拉塞尔塔的猩红之眼腰带", "引导拉塞尔塔传奇目光的腰带。没有目标能逃脱它的监视之力。");
        Add(452, "米斯特的决斗者冠军腰带", "阿斯特里德家族的终极腰带。米斯特的荣誉编织在每一根线中。");
        Add(453, "娜琪亚的世界树腰带", "从世界树本身生长而成的腰带。自然的愤怒流经其中。");
        Add(454, "空壳的不破之链", "由不可摧毁的决心锻造的腰带。空壳的意志就是它的力量。");
        Add(455, "空壳的暗影刺客腰带", "完美黑暗的腰带。空壳的操控者永远无法打破它的意志。");
        Add(456, "维斯珀的黄昏审判官腰带", "艾尔教团的最高等级腰带。它审判噩梦并驱逐黑暗。");
        Add(457, "尤巴的祖先冠军腰带", "召唤尤巴祖先的腰带。他们的力量流经每一根纤维。");
        
        // Universal Legendary Ring (458)
        Add(458, "永恒梦境之环", "将佩戴者连接到无限梦境领域的传奇戒指。它的力量超越所有界限。");
        
        // Hero-Specific Legendary Rings (459-467)
        Add(459, "奥蕾娜的大贤者之环", "月华奥术会大贤者的戒指。奥蕾娜将其作为他们虚伪的证据。");
        Add(460, "铋晶的宝石之心之环", "纯净水晶魔法的戒指。它随着盲女转化的精华而脉动。");
        Add(461, "拉塞尔塔的皇家处刑者之环", "皇家卫队精英处刑者的戒指。拉塞尔塔通过无数战斗赢得了它。");
        Add(462, "米斯特的阿斯特里德传承之环", "阿斯特里德家族的祖传戒指。米斯特的血脉流经它的金属。");
        Add(463, "娜琪亚的森林守护者之环", "被娜琪亚森林的精灵祝福的戒指。自然的力量是它的精华。");
        Add(464, "空壳的不屈意志之环", "由不可摧毁的意志锻造的戒指。空壳的决心就是它的力量。");
        Add(465, "空壳的完美暗影之环", "绝对黑暗的戒指。空壳的操控者永远无法控制它的力量。");
        Add(466, "维斯珀的圣焰之环", "包含艾尔圣焰纯净精华的戒指。它驱逐所有黑暗。");
        Add(467, "尤巴的部落冠军之环", "引导尤巴祖先力量的戒指。他们的愤怒赋予每一次攻击力量。");
        
        // Universal Legendary Amulet (468)
        Add(468, "无限梦境护符", "将佩戴者与永恒梦境循环绑定的传奇护符。它的力量超越所有界限。");
        
        // Hero-Specific Legendary Amulets (469-477)
        Add(469, "奥蕾娜的生命编织者护符", "由奥蕾娜被窃取的生命力研究锻造的护符。它将生与死编织成完美的平衡。");
        Add(470, "铋晶的水晶之心", "纯净水晶制成的护符，随着盲女视野的脉动。它能看到眼睛无法看到的东西。");
        Add(471, "拉塞尔塔的猩红之眼护符", "引导拉塞尔塔传奇目光的护符。没有目标能逃脱它的监视之力。");
        Add(472, "米斯特的决斗者冠军护符", "阿斯特里德家族的终极护符。米斯特的荣誉编织在每一颗宝石中。");
        Add(473, "娜琪亚的世界树护符", "从世界树本身生长而成的护符。自然的愤怒流经其中。");
        Add(474, "空壳的不破护符", "由不可摧毁的决心锻造的护符。空壳的意志就是它的力量。");
        Add(475, "空壳的暗影刺客护符", "完美黑暗的护符。空壳的操控者永远无法打破它的意志。");
        Add(476, "维斯珀的黄昏审判官护符", "艾尔教团的最高等级护符。它审判噩梦并驱逐黑暗。");
        Add(477, "尤巴的祖先冠军护符", "召唤尤巴祖先的护符。他们的力量流经每一颗宝石。");
    }
    
    private static void Add(int id, string nameCN, string descCN)
    {
        _nameCN[id] = nameCN;
        _descCN[id] = descCN;
    }
    
    // ============================================================
    // POLISH TRANSLATIONS
    // ============================================================
    private static void InitializePL()
    {
        _namePL = new Dictionary<int, string>();
        _descPL = new Dictionary<int, string>();
        
        // Aurena Weapons (1-18)
        AddPL(1, "Szpony Nowicjusza", "Proste szpony wręczane nowym członkom Towarzystwa Arkanum Księżycowego. Aurena zachowała swoje nawet po wygnaniu.");
        AddPL(2, "Szpony Uczonego", "Szpony ostrzone na zakazanych tekstach. Zostawiane przez nie zadrapania szepczą sekrety.");
        AddPL(3, "Szpony Księżycowe", "Szpony, które słabo świecą w świetle księżyca. Przekazują energię księżycową w niszczycielskie ciosy.");
        AddPL(4, "Brzytew Arkanum", "Szpony pokryte runami leczniczymi - teraz używane do rozrywania zamiast leczenia.");
        AddPL(5, "Szpony Wygnańca", "Wykute w tajemnicy po jej wygnaniu. Każde cięcie przypomina o tym, co Towarzystwo jej odebrało.");
        AddPL(6, "Uchwyt Heretyka", "Szpony, które kiedyś należały do innego wygnanego mędrca. Ich duch prowadzi ciosy Aureny.");
        AddPL(7, "Szpony Ofiarne", "Szpony, które stają się ostrzejsze, gdy siła życiowa Aureny słabnie. Towarzystwo uznało te badania za zbyt niebezpieczne.");
        AddPL(8, "Szpony Mrozu", "Szpony zamrożone lodem z najgłębszego skarbca Towarzystwa. Ich dotyk paraliżuje ciało i ducha.");
        AddPL(9, "Rozdzieracze Księżycowe", "Wzmocnione zakazanymi zaklęciami. Każda rana, którą zadają, leczy sojuszników Aureny.");
        AddPL(10, "Promienne Szpony", "Szpony pobłogosławione skradzionym światłem słonecznym. Badania Aureny nad magią światła rozgniewały zarówno kult słońca, jak i księżyca.");
        AddPL(11, "Szpony Żaru", "Szpony zapalone siłą życiową wyciągniętą z chętnych śniących. Płomienie nigdy nie gasną.");
        AddPL(12, "Szpony Cienia", "Szpony zanurzone w esencji koszmarów. Aurena nauczyła się, że ciemność leczy to, czego światło nie może.");
        AddPL(13, "Żniwiarz Lodowcowy", "Szpony wyrzeźbione z wiecznego lodu. Zamrażają krew wrogów i zachowują siłę życiową Aureny.");
        AddPL(14, "Szpony Feniksa", "Szpony wykute w płomieniach feniksa i hartowane własną krwią Aureny. Płoną nieugaszonym gniewem.");
        AddPL(15, "Rozdzieracz Pustki", "Szpony zdolne rozrywać samą rzeczywistość. Towarzystwo obawia się, co Aurena odkryje dalej.");
        AddPL(16, "Szpony Niebiańskie", "Szpony nasycone czystym światłem gwiazd. Każdy cios jest modlitwą o wiedzę, którą bogowie chcą ukryć.");
        AddPL(17, "Szpony Wielkiego Mędrca", "Ceremonialne szpony wielkiego mędrca Towarzystwa. Aurena używa ich jako dowodu ich obłudy.");
        AddPL(18, "Szpony Tkacza Życia", "Arcydzieło Aureny - szpony zdolne kraść siłę życiową bez jej niszczenia. Towarzystwo nazywa to herezją.");
        
        // Bismuth Weapons (19-36)
        AddPL(19, "Pusty Grimuar", "Pusta księga zaklęć czekająca na wypełnienie. Jak sama Bismuth, jej strony kryją nieskończony potencjał.");
        AddPL(20, "Księga Ucznia", "Księga zaklęć dla początkujących. Niewidoma dziewczyna, która stała się Bismuth, palcami śledziła jej strony, marząc o magii.");
        AddPL(21, "Kodeks Kryształowy", "Księga, której strony wykonano z cienkich klejnotów. Rezonuje z krystaliczną naturą Bismuth.");
        AddPL(22, "Dziennik Wędrowca", "Dziennik podróży zapisujący obserwacje niewidomej dziewczyny, która nigdy nie widziała, ale zawsze wiedziała.");
        AddPL(23, "Wprowadzenie do Klejnotów", "Podstawowy przewodnik po magii kryształów. Jej słowa migoczą jak tęczowe ciało Bismuth.");
        AddPL(24, "Klasyka Niewidomego", "Księga napisana wypukłymi literami dla niewidomych. Bismuth czyta ją dotykiem i pamięcią.");
        AddPL(25, "Księga Słów Żaru", "Księga, której strony wiecznie płoną. Płomienie rozmawiają z Bismuth kolorami, które czuje, a nie widzi.");
        AddPL(26, "Słownik Lodowy", "Księga owinięta wiecznym lodem. Jej lodowate strony zachowują wiedzę sprzed początku snów.");
        AddPL(27, "Rękopis Promienny", "Księga emitująca wewnętrzne światło. Przed tym, jak stała się Bismuth, prowadziła niewidomą dziewczynę przez ciemność.");
        AddPL(28, "Grimuar Cienia", "Księga pochłaniająca światło. Jej ciemne strony zawierają sekrety, których nawet Bismuth boi się czytać.");
        AddPL(29, "Żywa Księga Zaklęć", "Świadoma księga, która wybrała połączenie z niewidomą dziewczyną. Razem stały się czymś większym.");
        AddPL(30, "Kodeks Pryzmatyczny", "Księga rozkładająca magię na składowe kolory. Bismuth widzi świat przez światło, które załamuje.");
        AddPL(31, "Kronika Pustki", "Księga zapisująca wspomnienia tych, którzy zginęli w ciemności. Bismuth czyta ich historie, aby ich uczcić.");
        AddPL(32, "Grimuar Serca Klejnotu", "Pierwotna księga zaklęć, która połączyła się z niewidomą dziewczyną. Jej strony pulsują z krystalicznym życiem.");
        AddPL(33, "Księga Oślepiającego Światła", "Księga tak oślepiająca, że może oślepić widzących. Dla Bismuth jest tylko ciepłem.");
        AddPL(34, "Rocznik Mrozu", "Starożytna księga zamrożona w czasie. Jej proroctwa opowiadają o klejnocie chodzącym jak dziewczyna.");
        AddPL(35, "Księga Bez Imienia", "Księga bez imienia, jak dziewczyna bez wzroku. Razem znalazły swoją tożsamość.");
        AddPL(36, "Klasyka Piekielna", "Księga płonąca pasją śniącego, który odmawia pozwolenia, by ślepota go definiowała.");
        
        // Lacerta Weapons (37-54)
        AddPL(37, "Karabin Rekruta", "Standardowa broń wydawana rekrutom Gwardii Królewskiej. Lacerta opanował ją przed końcem pierwszego tygodnia szkolenia.");
        AddPL(38, "Długa Broń Myśliwska", "Niezawodny karabin myśliwski. Lacerta używał podobnego, aby utrzymać rodzinę przed dołączeniem do Gwardii.");
        AddPL(39, "Karabin Patrolowy", "Kompaktowy i niezawodny. Idealny do długich patroli na granicach królestwa.");
        AddPL(40, "Broń Strzelca Wyborowego", "Zbalansowany karabin zaprojektowany dla tych, którzy cenią precyzję ponad moc.");
        AddPL(41, "Muszkiet Prochowy", "Stary, ale niezawodny. Zapach czarnego prochu przypomina Lacercie jego pierwsze polowanie.");
        AddPL(42, "Karabin Zwiadowczy", "Lekki karabin do zadań zwiadowczych. Prędkość i ukrycie przewyższają surową moc.");
        AddPL(43, "Karabin Gwardii Królewskiej", "Broń, którą Lacerta nosił podczas służby. Każde zadrapanie opowiada historię.");
        AddPL(44, "Duma Snajpera", "Precyzyjny karabin dla tych, którzy nigdy nie chybiają. Szkarłatne oko Lacerty widzi to, czego inni nie mogą.");
        AddPL(45, "Długa Broń Płomienna", "Załadowana pociskami ognistymi. Wrogowie płoną długo po trafieniu.");
        AddPL(46, "Karabin Mrozu", "Wystrzeliwuje pociski schłodzone do zera absolutnego. Cele zwalniają do ślimaczego tempa przed śmiertelnym ciosem.");
        AddPL(47, "Karabin Nocny", "Broń do polowania w ciemności. Jego strzały są ciche jak cień.");
        AddPL(48, "Muszkiet Świtu", "Pobłogosławiony światłem poranka. Jego strzały przebijają ciemność i oszustwo.");
        AddPL(49, "Szkarłatne Oko", "Nazwany na cześć legendarnego wzroku Lacerty. Żaden cel nie ucieknie przed wzrokiem tego karabinu.");
        AddPL(50, "Egzekutor Królewski", "Zarezerwowany dla elity Gwardii. Lacerta otrzymał ten karabin po uratowaniu królestwa.");
        AddPL(51, "Działo Słoneczne", "Karabiny wystrzeliwujące skoncentrowane światło. Każdy strzał to miniaturowe wschód słońca.");
        AddPL(52, "Łowca Pustki", "Karabiny wystrzeliwujące czysto ciemne kule. Cele znikają bez śladu.");
        AddPL(53, "Zero Absolutne", "Najzimniejszy punkt istnienia. Nawet czas zamarza przed nim.");
        AddPL(54, "Oddech Smoka", "Karabiny załadowane wybuchowymi pociskami zapalającymi. Wrogowie królestwa nauczyli się bać jego ryku.");
        
        // Mist Weapons (55-70)
        AddPL(55, "Szpada Treningowa", "Miecz treningowy z młodości Mist. Nawet wtedy przewyższała każdego instruktora w Astrid.");
        AddPL(56, "Szpada Szlachecka", "Szpada pasująca do szlacheckiego pochodzenia Mist. Lekka, elegancka, śmiertelna.");
        AddPL(57, "Szpada Pojedynkowa", "Szpada zaprojektowana do walki jeden na jednego. Mist nigdy nie przegrała pojedynku.");
        AddPL(58, "Szpada Szybka", "Lekka jak przedłużenie ramienia Mist.");
        AddPL(59, "Szpada Astridzka", "Wykuta w najlepszych kuźniach Astrid. Symbol królewskiego rzemiosła.");
        AddPL(60, "Szpada Tancerza", "Szpada dla tych, którzy traktują walkę jako sztukę. Ruchy Mist są poetyczne.");
        AddPL(61, "Szpada Płomienna", "Szpada otoczona płomieniami. Gniew Mist jest tak gorący jak jej determinacja.");
        AddPL(62, "Szpada Cienia", "Szpada uderzająca z ciemności. Wrogowie padają, zanim zobaczą, jak Mist się porusza.");
        AddPL(63, "Szpada Mrozu", "Szpada z zamrożonej stali. Jej dotyk paraliżuje ciało i ducha.");
        AddPL(64, "Łaska Camilli", "Prezent od kogoś, kogo Mist ceniła. Walczy, aby uczcić ich pamięć.");
        AddPL(65, "Szpada Parująca", "Szpada zaprojektowana do obrony i ataku. Mist zamienia każdy atak w okazję.");
        AddPL(66, "Szpada Promienna", "Szpada emitująca wewnętrzne światło. Przebija kłamstwa i cienie.");
        AddPL(67, "Szpada Piekielna", "Szpada płonąca nieugaszonym zapałem. Determinacji Mist nie można ugasić.");
        AddPL(68, "Szpada Północna", "Szpada wykuta w absolutnej ciemności. Uderza ciszą śmierci.");
        AddPL(69, "Szpada Zimowa", "Szpada wyrzeźbiona z wiecznego lodu. Jej chłód może równać się tylko z koncentracją Mist w walce.");
        AddPL(70, "Szpada Nieugięta", "Legendarna szpada największego pojedynkowicza Astrid. Mist zdobyła ten tytuł przez niezliczone zwycięstwa.");
        
        // Nachia Weapons (71-88)
        AddPL(71, "Laska Sadzonki", "Młoda gałąź z lasu duchowego. Nawet sadzonki odpowiadają na wezwanie Nachii.");
        AddPL(72, "Przywoływacz Duchów", "Laska rzeźbiona, aby kierować głosami duchów lasu. Szeptają sekrety do Nachii.");
        AddPL(73, "Gałąź Lasu", "Żywa gałąź, która wciąż rośnie. Magia lasu przepływa przez nią.");
        AddPL(74, "Laska Dzikiej Natury", "Dzika i nieprzewidywalna, jak sama Nachia. Duchy uwielbiają jej chaotyczną energię.");
        AddPL(75, "Laska Strażnika", "Noszone przez tych, którzy chronią las. Nachia urodziła się z tym obowiązkiem.");
        AddPL(76, "Różdżka Duchowego Świata", "Różdżka łącząca świat materialny i duchowy. Nachia chodzi między nimi.");
        AddPL(77, "Laska Mroźnego Drewna", "Laska zamrożona w wiecznej zimie. Zimne duchy północy odpowiadają na jej wezwanie.");
        AddPL(78, "Laska Korzenia Cienia", "Rosnąca w najgłębszych częściach lasu. Duchy cienia tańczą wokół niej.");
        AddPL(79, "Laska Drewna Żaru", "Laska, która nigdy nie przestaje płonąć. Duchy ognia przyciąga jej ciepło.");
        AddPL(80, "Laska Błogosławieństwa Słońca", "Pobłogosławiona przez duchy świtu. Jej światło prowadzi zagubione dusze do domu.");
        AddPL(81, "Kły Fenrira", "Laska z kłami wilka ducha na szczycie. Wierny towarzysz Nachii kieruje jej mocą.");
        AddPL(82, "Laska Starego Dębu", "Wyrzeźbiona z tysiącletniego dębu. Najstarsze duchy pamiętają, kiedy został zasadzony.");
        AddPL(83, "Gałąź Drzewa Świata", "Gałąź z legendarnego Drzewa Świata. Wszystkie duchy lasu kłaniają się przed nią.");
        AddPL(84, "Laska Króla Duchów", "Laska samego Króla Duchów. Nachia zdobyła ją, chroniąc las.");
        AddPL(85, "Laska Wiecznego Mrozu", "Laska z wiecznego lodu z zamarzniętego serca lasu. Zimowe duchy podporządkowują się jej rozkazom.");
        AddPL(86, "Gniazdo Feniksa", "Laska, na której gnieżdżą się duchy ognia. Jej płomienie przynoszą odrodzenie, nie zniszczenie.");
        AddPL(87, "Laska Promiennego Gaju", "Laska emitująca światło tysiąca świetlików. Duchy nadziei tańczą w niej.");
        AddPL(88, "Laska Strażnika Pustki", "Laska strzegąca granic świata. Duchy cienia chronią jej posiadacza.");
        
        // Shell Weapons (89-106)
        AddPL(89, "Miecz Nowicjusza", "Podstawowy miecz samurajski dla uczących się drogi miecza. Shell opanował go w ciągu kilku dni.");
        AddPL(90, "Miecz Ciszy", "Miecz samurajski zaprojektowany, aby nie wydawać dźwięku. Ofiary Shell nigdy nie słyszą nadchodzącej śmierci.");
        AddPL(91, "Miecz Zabójcy", "Wąski i precyzyjny. Shell potrzebuje tylko jednego ciosu w odpowiednie miejsce.");
        AddPL(92, "Miecz Zabójczy", "Skuteczny. Praktyczny. Bezlitosny. Jak sam Shell.");
        AddPL(93, "Miecz Łowcy", "Miecz samurajski do polowania. Shell nie przestaje, dopóki cel nie zostanie wyeliminowany.");
        AddPL(94, "Miecz Pustki", "Nazwany na cześć pustki, którą Shell czuje. Lub nie czuje.");
        AddPL(95, "Miecz Egzekutora", "Miecz samurajski kończący niezliczone życia. Shell nie czuje nic do żadnego z nich.");
        AddPL(96, "Miecz Precyzyjny", "Miecz samurajski dostrojony, aby wykorzystywać słabości. Shell oblicza optymalny atak.");
        AddPL(97, "Miecz Mrozu", "Miecz samurajski zimnej złośliwości. Jego chłód pasuje do pustego serca Shell.");
        AddPL(98, "Miecz Oczyszczenia", "Jasny miecz samurajski do ciemnej pracy. Shell nie odczuwa ironii.");
        AddPL(99, "Miecz Cienia", "Miecz samurajski pijący cienie. Shell porusza się niewidocznie, uderza bezgłośnie.");
        AddPL(100, "Sztylet Płomienny", "Miecz samurajski podgrzany wewnętrznym ogniem. Cele Shell palą się przed krwawieniem.");
        AddPL(101, "Narzędzie Doskonałego Zabójcy", "Ostateczne narzędzie ostatecznego zabójcy. Shell urodził się dla tego miecza.");
        AddPL(102, "Ostrze Bezlitosne", "Miecz samurajski, który nigdy się nie tępi, dzierżony przez nieustającego prześladowcę.");
        AddPL(103, "Zabójca Piekielny", "Miecz samurajski wykuty w ogniu piekielnym. Płonie intensywnością jedynego celu Shell.");
        AddPL(104, "Miecz Pustki", "Miecz samurajski absolutnej ciemności. Jak Shell, istnieje tylko po to, aby kończyć.");
        AddPL(105, "Miecz Zamrożony", "Miecz samurajski z wiecznego lodu. Jego wyrok jest tak zimny i końcowy jak Shell.");
        AddPL(106, "Promienne Zakończenie", "Ostateczna forma jasnego miecza samurajskiego. Shell wykonuje sąd w imię El.");
        
        // Vesper Weapons (107-124)
        AddPL(107, "Buława Nowicjusza", "Prosta buława dla nowych członków Zakonu Płomienia Słońca. Vesper zaczęła tutaj swoją podróż.");
        AddPL(108, "Młot Sądu", "Młot bojowy używany do wykonywania woli El. Każde uderzenie to święty wyrok.");
        AddPL(109, "Kij Strażnika Ognia", "Broń noszona przez tych, którzy strzegą świętego ognia. Jego uderzenia oczyszczają nieczystość.");
        AddPL(110, "Tarcza Acolyte", "Prosta tarcza akolity Zakonu. Wiara jest najlepszą obroną.");
        AddPL(111, "Tarcza Inkwizytora", "Tarcza noszona przez inkwizytorów. Widziała upadek niezliczonych heretyków.");
        AddPL(112, "Obrońca Zmierzchu", "Tarcza strzegąca między światłem a ciemnością.");
        AddPL(113, "Młot Fanatyka", "Młot bojowy płonący religijnym fanatyzmem. Wrogowie drżą przed nim.");
        AddPL(114, "Ciężki Młot Oczyszczający", "Ogromny młot bojowy używany do oczyszczania zła. Jedno uderzenie kruszy grzech.");
        AddPL(115, "Wyrok El", "Młot bojowy pobłogosławiony osobiście przez El. Jego wyrok jest nieodwołalny.");
        AddPL(116, "Bastion Wiary", "Niezniszczalna tarcza. Dopóki wiara nie zgaśnie, nie pęknie.");
        AddPL(117, "Święta Tarcza Płomienia", "Tarcza otoczona świętym ogniem. Zło dotykające jej zostaje spalone.");
        AddPL(118, "Bastion Inkwizycji", "Tarcza wysokiego inkwizytora. Vesper zdobyła ją przez bezlitosną służbę.");
        AddPL(119, "Rozbijacz Świtu", "Legendarny młot bojowy kruszący ciemność. Vesper dzierży ucieleśnienie gniewu El.");
        AddPL(120, "Święty Niszczyciel", "Młot bojowy pobłogosławiony przez najwyższego kapłana. Jego uderzenia niosą ciężar świętego gniewu.");
        AddPL(121, "Prawa Ręka El", "Najświętsza broń Zakonu. Vesper jest wybranym narzędziem woli El.");
        AddPL(122, "Forteca Płomienia", "Ostateczna obrona Zakonu. Światło El chroni Vesper przed wszelką krzywdą.");
        AddPL(123, "Wieczna Czujność", "Nieugięta tarcza. Jak Vesper, zawsze czuwa nad ciemnością.");
        AddPL(124, "Przysięga Zmierzchowego Paladyna", "Tarcza przywódcy Zakonu. Vesper przysięgła związać swoją duszę z El.");
        
        // Yubar Weapons (125-142)
        AddPL(125, "Nowonarodzona Gwiazda", "Gwiazda, która dopiero się formuje. Yubar pamięta, kiedy wszystkie gwiazdy były tak małe.");
        AddPL(126, "Żar Kosmiczny", "Fragment ognia gwiazdowego. Płonie ciepłem samego stworzenia.");
        AddPL(127, "Kula Pyłu Gwiezdnego", "Skompresowany pył gwiezdny czekający na uformowanie. Yubar tka galaktyki z takiej materii.");
        AddPL(128, "Katalizator Snów", "Kula wzmacniająca energię snów. Pochodzi z pierwszych eksperymentów Yubara.");
        AddPL(129, "Rdzeń Mgławicy", "Serce odległej mgławicy. Yubar wyrwał je z kosmicznego gobelinu.");
        AddPL(130, "Nasiono Niebieskie", "Nasiono, które stanie się gwiazdą. Yubar hoduje niezliczone takie nasiona.");
        AddPL(131, "Fragment Supernowej", "Fragment eksplodującej gwiazdy. Yubar był świadkiem jej śmierci i zachował jej moc.");
        AddPL(132, "Studnia Grawitacyjna", "Kula kompresująca grawitację. Przestrzeń sama się wokół niej zakrzywia.");
        AddPL(133, "Osobliwość Pustki", "Punkt nieskończonej gęstości. Yubar używa go do tworzenia i niszczenia światów.");
        AddPL(134, "Korona Słoneczna", "Fragment zewnętrznej atmosfery słońca. Płonie gniewem gwiazdy.");
        AddPL(135, "Zamrożona Kometa", "Złapane jądro komety. Niesie sekrety z głębi kosmosu.");
        AddPL(136, "Piec Gwiazdowy", "Miniatura jądra gwiazdy. Yubar używa go do kucia nowej rzeczywistości.");
        AddPL(137, "Relikt Wielkiego Wybuchu", "Fragment Wielkiego Wybuchu stworzenia. Zawiera pochodzenie wszechświata.");
        AddPL(138, "Warsztat Kosmiczny", "Narzędzie tkające rzeczywistość. Yubar używa go do przekształcania przeznaczenia.");
        AddPL(139, "Promienne Stworzenie", "Samo światło stworzenia. Yubar użył go, aby zrodzić pierwsze gwiazdy.");
        AddPL(140, "Serce Entropii", "Esencja rozpadu kosmosu. Wszystko się kończy, a Yubar decyduje kiedy.");
        AddPL(141, "Zero Absolutne", "Najzimniejszy punkt istnienia. Nawet czas zamarza przed nim.");
        AddPL(142, "Rdzeń Katastrofy", "Serce zniszczenia kosmosu. Yubar uwalnia go tylko w krytycznych momentach.");
        
        // Rings (143-162)
        AddPL(143, "Pierścień Marzyciela", "Prosty pierścień noszony przez tych, którzy dopiero wchodzą w świat snów. Emituje słabą nadzieję.");
        AddPL(144, "Pierścień Pyłu Gwiezdnego", "Wykuty ze skondensowanego pyłu gwiezdnego. Marzyciele mówią, że świeci jaśniej w niebezpieczeństwie.");
        AddPL(145, "Znak Snu", "Wyryty znak spokojnego snu. Noszący szybciej wracają do zdrowia z ran.");
        AddPL(146, "Pierścień Ducha", "Pierścień chwytający błąkające się duchy. Ich światło prowadzi ścieżki przez ciemne sny.");
        AddPL(147, "Pierścień Wędrowca", "Noszone przez wędrowców dziedzin snów. Widział niezliczone zapomniane ścieżki.");
        AddPL(148, "Pierścień Fragmentu Koszmaru", "Pierścień zawierający fragmenty pokonanych koszmarów. Jego ciemność wzmacnia odważnych.");
        AddPL(149, "Strażnik Wspomnień", "Pierścień zachowujący cenne wspomnienia. Smoothie sprzedaje podobne w swoim sklepie.");
        AddPL(150, "Pierścień Arkanum Księżycowego", "Pierścień z Towarzystwa Arkanum Księżycowego. Aurena nosiła podobny przed wygnaniem.");
        AddPL(151, "Znak Lasu Duchowego", "Pierścień pobłogosławiony przez duchy lasu Nachii. Siła natury przepływa przez niego.");
        AddPL(152, "Odznaka Gwardii Królewskiej", "Pierścień noszony przez Lacertę podczas służby. Wciąż niesie ciężar odpowiedzialności.");
        AddPL(153, "Chwała Pojedynkowicza", "Pierścień przekazywany wśród szlacheckich pojedynkowiczów. Rodzina Mist ceni takie pierścienie.");
        AddPL(154, "Amulet Żaru Płomienia", "Zapieczętowana iskra świętego płomienia. Zakon Vesper strzeże tych relikwii.");
        AddPL(155, "Fragment Księgi Zaklęć", "Pierścień wykonany z krystalicznej magii Bismuth. Bzyczy energią arkanum.");
        AddPL(156, "Znak Łowcy", "Pierścień śledzący zdobycz. Kontrolerzy Shell używają ich do monitorowania swoich narzędzi.");
        AddPL(157, "Błogosławieństwo El", "Pierścień pobłogosławiony przez samo El. Jego światło rozprasza koszmary i leczy rannych.");
        AddPL(158, "Znak Władcy Koszmarów", "Zdobyty od pokonanego władcy koszmarów. Jego ciemna moc korumpuje i wzmacnia.");
        AddPL(159, "Dziedzictwo Astrid", "Pierścień z szlacheckiego domu Astrid. Przodkowie Mist nosili go w niezliczonych pojedynkach.");
        AddPL(160, "Oko Widzącego", "Pierścień zdolny widzieć przez zasłonę snów. Przeszłość, teraźniejszość i przyszłość mieszają się.");
        AddPL(161, "Pierścień Pierwotnego Snu", "Wykuty w świtaniu snów. Zawiera esencję pierwszego marzyciela.");
        AddPL(162, "Wieczny Sen", "Pierścień dotykający najgłębszego snu. Noszący nie boi się ani śmierci, ani przebudzenia.");
        
        // Amulets (163-183)
        AddPL(163, "Wisiorek Marzyciela", "Prosty wisiorek noszony przez tych, którzy dopiero wchodzą w świat snów. Emituje słabą nadzieję.");
        AddPL(164, "Wisiorek Dotyku Ognia", "Wisiorek dotknięty ogniem. Jego ciepło rozprasza chłód i strach.");
        AddPL(165, "Szkarłatna Łza", "Rubin w kształcie krwawej łzy. Jego moc pochodzi z poświęcenia.");
        AddPL(166, "Amulet Bursztynowy", "Bursztyn zawierający starożytne stworzenie. Jego wspomnienia wzmacniają noszącego.");
        AddPL(167, "Medal Zmierzchu", "Medal migający między światłem a ciemnością. Nowi członkowie Vesper noszą te.");
        AddPL(168, "Naszyjnik Pyłu Gwiezdnego", "Naszyjnik posypany krystalicznymi gwiazdami. Smoothie zbiera je z upadłych snów.");
        AddPL(169, "Oko Szmaragdowe", "Zielony klejnot zdolny widzieć przez iluzje. Koszmary nie mogą uniknąć jego spojrzenia.");
        AddPL(170, "Serce Ducha Lasu", "Klejnot zawierający błogosławieństwo duchów lasu. Strażnicy Nachii cenią te.");
        AddPL(171, "Znak Arkanum Księżycowego", "Znak z zakazanych archiwów. Aurena ryzykuje wszystko, badając te.");
        AddPL(172, "Kryształ Bismuth", "Fragment krystalicznej czystej magii. Rezonuje z energią arkanum.");
        AddPL(173, "Amulet Żaru Płomienia", "Zapieczętowana iskra świętego płomienia. Zakon Vesper strzeże tych relikwii.");
        AddPL(174, "Wisiorek Kła Koszmaru", "Kieł z potężnego koszmaru, teraz trofeum. Shell nosi jeden jako przypomnienie.");
        AddPL(175, "Medal Gwardii Królewskiej", "Medal honorowy z Gwardii Królewskiej. Lacerta zdobył wiele przed wygnaniem.");
        AddPL(176, "Herb Astrid", "Szlachecki herb rodziny Astrid. Dziedzictwo rodziny Mist wisi na tym łańcuchu.");
        AddPL(177, "Święta Łza El", "Łza spadająca z samego El. Jego światło chroni przed najciemniejszymi koszmarami.");
        AddPL(178, "Szmaragd Pierwotny", "Klejnot z pierwszego lasu. Zawiera sny starożytnych duchów.");
        AddPL(179, "Oko Władcy Koszmarów", "Oko pokonanego władcy koszmarów. Widzi wszystkie strachy i wykorzystuje je.");
        AddPL(180, "Serce Płomienia", "Rdzeń samego świętego płomienia. Tylko najbardziej pobożni mogą go nosić.");
        AddPL(181, "Wyrocznia Widzącego", "Amulet zdolny zobaczyć wszystkie możliwe przyszłości. Rzeczywistość ugina się przed jego proroctwami.");
        AddPL(182, "Kompas Wędrowca Pustki", "Amulet wskazujący pustkę. Ci, którzy podążają za nim, nigdy nie wracają w ten sam sposób.");
        AddPL(183, "Amulet Midasa", "Legendarny amulet dotknięty snem złota. Pokonani wrogowie upuszczają dodatkowe złoto.");
        AddPL(478, "Zbieracz Gwiezdnego Pyłu", "Legendarny amulet zbierający skrystalizowany gwiezdny pył z pokonanych wrogów. Każde zabójstwo daje cenną esencję snów (podstawa 1-5, skaluje się z poziomem ulepszenia i siłą potwora).");
        
        // Belts (184-203)
        AddPL(184, "Pas Wędrowca", "Prosty pas z tkaniny noszony przez tych, którzy wędrują między snami. Kotwiczy duszę.");
        AddPL(185, "Lina Marzyciela", "Lina utkana z nici snów. Delikatnie pulsuje z każdym uderzeniem serca.");
        AddPL(186, "Pas Pyłu Gwiezdnego", "Pas ozdobiony krystalicznym pyłem gwiezdnym. Smoothie sprzedaje podobne akcesoria.");
        AddPL(187, "Pas Wartownika", "Solidny pas noszony przez strażników snów. Nigdy się nie poluzowuje, nigdy nie zawodzą.");
        AddPL(188, "Pas Tajemniczy", "Pas nasycony słabą magią. Kłuje, gdy koszmary się zbliżają.");
        AddPL(189, "Pas Myśliwski", "Skórzany pas z torbami na zapasy. Niezbędny do długich podróży w snach.");
        AddPL(190, "Pas Pielgrzyma", "Noszone przez tych, którzy szukają światła El. Zapewnia pocieszenie w najciemniejszych snach.");
        AddPL(191, "Pas Łowcy Koszmarów", "Pas wykonany z pokonanych koszmarów. Ich esencja wzmacnia noszącego.");
        AddPL(192, "Pas Arkanum", "Pas z Towarzystwa Arkanum Księżycowego. Wzmacnia energię magiczną.");
        AddPL(193, "Pas Płomienia", "Pas pobłogosławiony przez Zakon Vesper. Płonie ogniem ochrony.");
        AddPL(194, "Pas Pojedynkowicza", "Elegancki pas noszony przez szlacheckich wojowników Astrid. Nosi miecze i honor.");
        AddPL(195, "Lina Tkacza Duchów", "Pas utkany przez duchy lasu dla Nachii. Bzyczy energią natury.");
        AddPL(196, "Pas Narzędzi Zabójcy", "Pas z ukrytymi przegrodami. Kontrolerzy Shell używają ich do wyposażania swoich narzędzi.");
        AddPL(197, "Pas Rewolwerowca", "Pas zaprojektowany dla broni palnej Lacerty. Szybkie wyciąganie, szybsze zabijanie.");
        AddPL(198, "Święty Pas El", "Pas pobłogosławiony przez samo El. Jego światło kotwiczy duszę przeciwko wszelkiej ciemności.");
        AddPL(199, "Łańcuch Władcy Koszmarów", "Pas wykuty z łańcuchów władcy koszmarów. Wiąże strach z wolą noszącego.");
        AddPL(200, "Pas Pierwotnego Snu", "Pas z pierwszego snu. Zawiera echo samego stworzenia.");
        AddPL(201, "Pas Korzenia Drzewa Świata", "Pas wyrastający z korzeni Drzewa Świata. Las Nachii pobłogosławił jego stworzenie.");
        AddPL(202, "Dziedzictwo Astrid", "Rodzinny pas rodziny Astrid. Krew Mist jest utkana w jego tkaninie.");
        AddPL(203, "Pas Zmierzchowego Inkwizytora", "Pas najwyższego poziomu Vesper. Sądzi wszystkich, którzy przed nim stoją.");
        
        // Armor Sets - Bone Sentinel (204-215)
        AddPL(204, "Maska Kościanego Wartownika", "Maska wyrzeźbiona z czaszki upadłego koszmaru. Jej puste oczy widzą przez iluzje.");
        AddPL(205, "Hełm Strażnika Duchowego Świata", "Hełm nasycony duchami lasu Nachii. Szepty ochrony otaczają noszącego.");
        AddPL(206, "Korona Władcy Koszmarów", "Korona wykuta z podbitych koszmarów. Noszący rządzi samym strachem.");
        AddPL(207, "Kamizelka Kościanego Wartownika", "Pancerz z żeber stworzeń, które zmarły we śnie. Ich ochrona trwa po śmierci.");
        AddPL(208, "Napierśnik Strażnika Duchowego Świata", "Napierśnik utkany ze skrystalizowanych snów. Porusza się i dostosowuje do nadchodzących ciosów.");
        AddPL(209, "Napierśnik Władcy Koszmarów", "Pancerz pulsujący ciemną energią. Koszmary kłaniają się przed jego noszącym.");
        AddPL(210, "Nagolenniki Kościanego Wartownika", "Nagolenniki wzmocnione kośćmi koszmarów. Nigdy się nie męczą, nigdy nie ustępują.");
        AddPL(211, "Nagolenniki Strażnika Duchowego Świata", "Nagolenniki pobłogosławione przez duchy lasu. Noszący porusza się jak wiatr przez liście.");
        AddPL(212, "Nagolenniki Władcy Koszmarów", "Nagolenniki pijące cienie. Ciemność wzmacnia każdy krok.");
        AddPL(213, "Buty Kościanego Wartownika", "Buty do cichego chodzenia w dziedzinach snów. Umarli nie wydają dźwięków.");
        AddPL(214, "Żelazne Buty Strażnika Duchowego Świata", "Buty, które nie pozostawiają śladów w dziedzinach snów. Idealne dla łowców koszmarów.");
        AddPL(215, "Buty Władcy Koszmarów", "Buty przekraczające światy. Rzeczywistość ugina się pod każdym krokiem.");
        
        // Aurena Armor (216-243)
        AddPL(216, "Czapka Ucznia Aureny", "Prosta czapka noszona przez nowicjuszy Arkanum Księżycowego.");
        AddPL(217, "Turban Tajemniczy Aureny", "Turban nasycony słabą energią księżycową.");
        AddPL(218, "Korona Uwodzicielska Aureny", "Korona wzmacniająca magiczne powinowactwo noszącego.");
        AddPL(219, "Korona Starożytna Aureny", "Starożytna relikwia przekazywana przez pokolenia.");
        AddPL(220, "Opaska Sokoła Aureny", "Opaska pobłogosławiona przez duchy nieba.");
        AddPL(221, "Korona Mocy Aureny", "Potężna korona emitująca moc arkanum.");
        AddPL(222, "Korona Księżycowa Aureny", "Legendarna korona mistrza Arkanum Księżycowego.");
        AddPL(223, "Niebieska Szata Aureny", "Prosta szata noszona przez magów księżycowych.");
        AddPL(224, "Szata Magiczna Aureny", "Szata ceniona przez praktykujących magów.");
        AddPL(225, "Szata Tkana Zaklęciami Aureny", "Szata utkana magicznymi nićmi.");
        AddPL(226, "Szata Oświecająca Aureny", "Szata prowadząca mądrość księżycową.");
        AddPL(227, "Szata Snów Aureny", "Szata utkana ze skrystalizowanych snów.");
        AddPL(228, "Szata Królewska Aureny", "Szata odpowiednia dla królewskiej rodziny księżycowej.");
        AddPL(229, "Szata Królowej Lodu Aureny", "Legendarna szata samej Królowej Lodu.");
        AddPL(230, "Niebieskie Nagolenniki Aureny", "Proste nagolenniki noszone przez aspirujących magów.");
        AddPL(231, "Spódnica Włóknista Aureny", "Lekka spódnica pozwalająca na swobodny ruch.");
        AddPL(232, "Nagolenniki Elitarne Aureny", "Eleganckie nagolenniki zaprojektowane przez elfy.");
        AddPL(233, "Nagolenniki Oświecające Aureny", "Nagolenniki pobłogosławione mądrością.");
        AddPL(234, "Spódnica Lodowcowa Aureny", "Spódnica nasycona magią lodu.");
        AddPL(235, "Spodnie Lodowe Aureny", "Spodnie emitujące siłę zimna.");
        AddPL(236, "Nagolenniki Wspaniałe Aureny", "Legendarne nagolenniki niezrównanej elegancji.");
        AddPL(237, "Miękkie Pantofle Księżycowe", "Miękkie buty noszone przez akolitów Arkanum Księżycowego. Amortyzują każdy krok światłem księżycowym.");
        AddPL(238, "Sandały Pielgrzyma", "Proste sandały pobłogosławione przez starszych Towarzystwa. Aurena wciąż pamięta ich ciepło.");
        AddPL(239, "Buty Wschodniej Tajemnicy", "Egzotyczne buty z odległych krain. Prowadzą uzdrawiającą energię przez ziemię.");
        AddPL(240, "Łaska Wędrowca Snów", "Buty chodzące między światem jawy a snów. Idealne dla uzdrowicieli szukających zagubionych dusz.");
        AddPL(241, "Buty Wędrowca Dusz", "Buty nasycone esencją zmarłych uzdrowicieli. Ich mądrość prowadzi każdy krok.");
        AddPL(242, "Buty Tropiciela Dusz", "Ciemne buty śledzące rannych. Aurena używa ich, aby znaleźć potrzebujących pomocy... lub tych, którzy ją skrzywdzili.");
        AddPL(243, "Skrzydła Upadłego Mędrca", "Legendarne buty, które podobno dają zdolność latania czystym sercom. Wygnanie Aureny udowodniło jej kwalifikacje.");
        
        // Bismuth Armor (244-271)
        AddPL(244, "Czapka Stożkowa Bismuth", "Prosta czapka dla krystalicznych uczniów.");
        AddPL(245, "Czapka Szmaragdowa Bismuth", "Czapka inkrustowana szmaragdowymi kryształami.");
        AddPL(246, "Maska Lodowcowa Bismuth", "Maska wykonana z zamrożonych kryształów.");
        AddPL(247, "Maska Alarharim Bismuth", "Starożytna maska mocy krystalicznej.");
        AddPL(248, "Hełm Pryzmatyczny Bismuth", "Hełm załamujący światło w tęczę.");
        AddPL(249, "Korona Odbijająca Bismuth", "Korona odbijająca energię magiczną.");
        AddPL(250, "Korona Białej Żarliwości Bismuth", "Legendarna korona płonąca ogniem kryształów.");
        AddPL(251, "Szata Lodowa Bismuth", "Szata schłodzona magią kryształów.");
        AddPL(252, "Szata Mnicha Bismuth", "Prosta szata do medytacji.");
        AddPL(253, "Szata Lodowcowa Bismuth", "Szata zamrożona w wiecznym lodzie.");
        AddPL(254, "Zbroja Kryształowa Bismuth", "Zbroja uformowana z żywych kryształów.");
        AddPL(255, "Zbroja Pryzmatyczna Bismuth", "Zbroja zakrzywiająca samo światło.");
        AddPL(256, "Płytowa Zbroja Lodowa Bismuth", "Płytowa zbroja wiecznego mrozu.");
        AddPL(257, "Święta Płytowa Zbroja Bismuth", "Legendarna zbroja pobłogosławiona przez krystalicznych bogów.");
        AddPL(258, "Nagolenniki Kolczugowe Bismuth", "Podstawowe nagolenniki z krystalicznymi ogniwami.");
        AddPL(259, "Nagolenniki Włókniste Bismuth", "Lekkie nagolenniki dla magów kryształów.");
        AddPL(260, "Nagolenniki Szmaragdowe Bismuth", "Nagolenniki ozdobione szmaragdowymi kryształami.");
        AddPL(261, "Spodnie Dziwne Bismuth", "Spodnie pulsujące dziwną energią.");
        AddPL(262, "Nagolenniki Pryzmatyczne Bismuth", "Nagolenniki migoczące pryzmatycznym światłem.");
        AddPL(263, "Nagolenniki Alarharim Bismuth", "Starożytne nagolenniki Alarharim.");
        AddPL(264, "Nagolenniki Sokoła Bismuth", "Legendarne nagolenniki Zakonu Sokoła.");
        AddPL(265, "Pantofle Uczonego", "Wygodne buty do długiej pracy w bibliotekach. Wiedza płynie przez ich podeszwy.");
        AddPL(266, "Owijacze Alarharim", "Starożytne owijacze z zaginionej cywilizacji. Szeptają zapomniane zaklęcia.");
        AddPL(267, "Buty Lodowe", "Buty wyrzeźbione z wiecznego lodu. Bismuth pozostawia wzory mrozu tam, gdzie chodzi.");
        AddPL(268, "Krok Mrozu", "Eleganckie buty kwitnące kryształami lodu. Każdy krok tworzy ogród mrozu.");
        AddPL(269, "Buty Rezonansu Kryształowego", "Buty wzmacniające energię magiczną. Kryształy bzyczą niewykorzystaną mocą.");
        AddPL(270, "Buty Arkanum Pryzmatycznego", "Buty załamujące światło w czystą energię magiczną. Rzeczywistość ugina się wokół każdego kroku.");
        AddPL(271, "Buty Wędrowca Pustki", "Legendarne buty przekraczające samą pustkę. Bismuth widzi ścieżki, których inni nie mogą sobie wyobrazić.");
        
        // Lacerta Armor (272-299)
        AddPL(272, "Skórzany Hełm Jaszczura", "Podstawowy hełm dla wojowników smoków.");
        AddPL(273, "Hełm Kolczugowy Jaszczura", "Solidny hełm kolczugowy.");
        AddPL(274, "Hełm Wojownika Jaszczura", "Hełm noszony przez doświadczonych wojowników.");
        AddPL(275, "Hełm Łuski Smoka Jaszczura", "Hełm wykuty z łusek smoka.");
        AddPL(276, "Hełm Królewski Jaszczura", "Hełm odpowiedni dla królewskiej rodziny smoków.");
        AddPL(277, "Elitarny Hełm Smoczego Człowieka Jaszczura", "Elitarny hełm Zakonu Smoczego Człowieka.");
        AddPL(278, "Złoty Hełm Jaszczura", "Legendarny złoty hełm Króla Smoków.");
        AddPL(279, "Skórzana Zbroja Jaszczura", "Podstawowa zbroja dla wojowników smoków.");
        AddPL(280, "Zbroja Łuskowa Jaszczura", "Zbroja wzmocniona łuskami.");
        AddPL(281, "Zbroja Rycerska Jaszczura", "Ciężka zbroja dla rycerzy smoków.");
        AddPL(282, "Kolczuga Łuski Smoka Jaszczura", "Kolczuga wykuta z łusek smoka.");
        AddPL(283, "Królewska Kolczuga Smoczego Człowieka Jaszczura", "Królewska kolczuga Zakonu Smoczego Człowieka.");
        AddPL(284, "Płytowa Zbroja Sokoła Jaszczura", "Płytowa zbroja Zakonu Sokoła.");
        AddPL(285, "Złota Zbroja Jaszczura", "Legendarna złota zbroja Króla Smoków.");
        AddPL(286, "Skórzane Nagolenniki Jaszczura", "Podstawowe nagolenniki dla wojowników smoków.");
        AddPL(287, "Nagolenniki Nitowane Jaszczura", "Nagolenniki wzmocnione nitami.");
        AddPL(288, "Nagolenniki Rycerskie Jaszczura", "Ciężkie nagolenniki dla rycerzy smoków.");
        AddPL(289, "Nagolenniki Łuski Smoka Jaszczura", "Nagolenniki wykute z łusek smoka.");
        AddPL(290, "Nagolenniki Królewskie Jaszczura", "Nagolenniki odpowiednie dla rodziny królewskiej.");
        AddPL(291, "Nagolenniki Wspaniałe Jaszczura", "Wspaniałe nagolenniki mistrzowskiego rzemiosła.");
        AddPL(292, "Złote Nagolenniki Jaszczura", "Legendarne złote nagolenniki Króla Smoków.");
        AddPL(293, "Zniszczone Buty Skórzane", "Proste buty, które przeszły wiele bitew. Niosą zapach przygody.");
        AddPL(294, "Buty Łatane", "Buty naprawiane niezliczoną ilość razy. Każda łatka opowiada historię przetrwania.");
        AddPL(295, "Buty Stalowego Wojownika", "Ciężkie buty wykute do walki. Kotwiczą Lacertę na ziemi podczas bitwy.");
        AddPL(296, "Nagolenniki Strażnika", "Buty noszone przez elitarnych strażników. Nigdy się nie cofają, nigdy się nie poddają.");
        AddPL(297, "Nagolenniki Łuski Smoka", "Buty wykonane z łusek smoka. Dają noszącemu wytrzymałość smoka.");
        AddPL(298, "Buty Wodza Smoczego Człowieka", "Buty starożytnych generałów smoczego człowieka. Groźne na każdym polu bitwy.");
        AddPL(299, "Buty Złotego Mistrza", "Legendarne buty największego wojownika epoki. Przeznaczenie Lacerty czeka.");
        
        // Mist Armor (300-327)
        AddPL(300, "Turban Mgły", "Prosty turban ukrywający tożsamość.");
        AddPL(301, "Lekki Turban Mgły", "Lekki, oddychający turban.");
        AddPL(302, "Turban Widzenia w Ciemności Mgły", "Turban wzmacniający widzenie w nocy.");
        AddPL(303, "Opaska Błyskawicy Mgły", "Opaska naładowana energią elektryczną.");
        AddPL(304, "Kaptur Kobry Mgły", "Kaptur śmiertelny jak wąż.");
        AddPL(305, "Maska Tropiciela Piekła Mgły", "Maska śledząca zdobycz.");
        AddPL(306, "Ciemny Szept Mgły", "Legendarna maska szepcząca śmierć.");
        AddPL(307, "Skórzana Uprząż Mgły", "Lekka uprząż do zwinności.");
        AddPL(308, "Płaszcz Łowcy Mgły", "Płaszcz do szybkiego ruchu.");
        AddPL(309, "Tunika Północna Mgły", "Tunika do nocnych uderzeń.");
        AddPL(310, "Szata Błyskawicy Mgły", "Szata naładowana błyskawicą.");
        AddPL(311, "Zbroja Napięciowa Mgły", "Zbroja pulsująca energią elektryczną.");
        AddPL(312, "Zbroja Mistrza Łucznika Mgły", "Zbroja mistrza łucznictwa.");
        AddPL(313, "Płaszcz Ciemnego Władcy Mgły", "Legendarny płaszcz Ciemnego Władcy.");
        AddPL(314, "Nagolenniki Łowcy Mgły", "Nagolenniki noszone przez łowców.");
        AddPL(315, "Nagolenniki Przetrwania w Dżungli Mgły", "Nagolenniki przetrwania w dżungli.");
        AddPL(316, "Sarong Północny Mgły", "Sarong do działań nocnych.");
        AddPL(317, "Nagolenniki Błyskawicy Mgły", "Nagolenniki naładowane błyskawicą.");
        AddPL(318, "Nagolenniki Szarańczy Mgły", "Nagolenniki do niesamowitych skoków.");
        AddPL(319, "Nagolenniki Zielonego Demona Mgły", "Nagolenniki demonicznej prędkości.");
        AddPL(320, "Nagolenniki Wędrowca Dusz Mgły", "Legendarne nagolenniki przekraczające światy.");
        AddPL(321, "Buty Zwiadowcze Tymczasowe", "Buty złożone z fragmentów. Lekkie i ciche, idealne do zwiadu.");
        AddPL(322, "Buty Biegacza Bagna", "Wodoodporne buty do trudnego terenu. Mist zna każdą skrót.");
        AddPL(323, "Szybki Wędrowiec", "Zaklęte buty przyspieszające kroki noszącego. Idealne do taktyki uderz i uciekaj.");
        AddPL(324, "Buty Łowcy Jelenia", "Buty śledzące zdobycz.");
        AddPL(325, "Buty Uderzenia Błyskawicy", "Buty naładowane energią błyskawicy.");
        AddPL(326, "Buty Bojowe Wędrowca Ognia", "Buty zdolne chodzić przez ogień.");
        AddPL(327, "Deptacz Cierpienia", "Legendarne buty depczące wszelkie cierpienie.");
        
        // Nachia Armor (328-355)
        AddPL(328, "Czapka Futrzana Nachii", "Prosta czapka dla myśliwych leśnych.");
        AddPL(329, "Nakrycie Głowy z Piórami Nachii", "Nakrycie głowy ozdobione piórami.");
        AddPL(330, "Maska Szamana Nachii", "Maska mocy duchowej.");
        AddPL(331, "Kaptur Ziemi Nachii", "Kaptur pobłogosławiony przez duchy ziemi.");
        AddPL(332, "Hełm Natury Nachii", "Hełm nasycony siłą natury.");
        AddPL(333, "Korona Drzewa Nachii", "Korona wyrastająca z starożytnego drzewa.");
        AddPL(334, "Korona Liści Nachii", "Legendarna korona Strażnika Lasu.");
        AddPL(335, "Zbroja Futrzana Nachii", "Zbroja wykonana z leśnych stworzeń.");
        AddPL(336, "Zbroja Rdzenna Nachii", "Tradycyjna zbroja leśnych plemion.");
        AddPL(337, "Płaszcz Zielonego Drewna Nachii", "Płaszcz utkany z żywych winorośli.");
        AddPL(338, "Płaszcz Ziemi Nachii", "Płaszcz pobłogosławiony przez duchy ziemi.");
        AddPL(339, "Uścisk Natury Nachii", "Zbroja zjednoczona z naturą.");
        AddPL(340, "Zbroja Gniazda Bagna Nachii", "Zbroja z głębokich bagien.");
        AddPL(341, "Szata Liści Nachii", "Legendarna szata Strażnika Lasu.");
        AddPL(342, "Spodenki z Futra Mamuta Nachii", "Ciepłe spodenki wykonane z futra mamuta.");
        AddPL(343, "Spodenki Skórzane Nachii", "Tradycyjne nagolenniki myśliwskie.");
        AddPL(344, "Nagolenniki Jelenia Nachii", "Nagolenniki wykonane ze skóry jelenia.");
        AddPL(345, "Nagolenniki Ziemi Nachii", "Nagolenniki pobłogosławione przez duchy ziemi.");
        AddPL(346, "Nagolenniki Liści Nachii", "Nagolenniki utkane z magicznych liści.");
        AddPL(347, "Przepaska Dzikiego Człowieka Nachii", "Przepaska z potężnego dzikiego człowieka.");
        AddPL(348, "Nagolenniki Krwawego Polowania", "Legendarne nagolenniki krwawego polowania.");
        AddPL(349, "Buty z Futrem Wewnętrznym", "Ciepłe, wygodne buty.");
        AddPL(350, "Buty ze Skóry Borsuka", "Buty wykonane ze skóry borsuka.");
        AddPL(351, "Buty Uderzenia Kobry", "Buty szybkie jak wąż.");
        AddPL(352, "Owijacze Skradacza Lasu", "Owijacze ciche w lesie.");
        AddPL(353, "Buty Myśliwskiego Ziemi", "Buty śledzące zdobycz.");
        AddPL(354, "Buty Łowcy Gorączki", "Buty ozdobione kwiatami gorączki. Dają gorączkową prędkość i śmiertelną precyzję.");
        AddPL(355, "Buty Krwawego Drapieżcy", "Legendarne buty przesiąknięte krwią niezliczonych polowań. Nachia stała się najwyższym drapieżcą.");
        
        // Shell Armor (356-383)
        AddPL(356, "Zniszczony Hełm Shell", "Zniszczony hełm z podziemnego świata.");
        AddPL(357, "Złamana Maska Shell", "Pęknięta maska wciąż zapewniająca ochronę.");
        AddPL(358, "Hełm Władcy Kości Shell", "Hełm wykuty ze szczątków Władcy Kości.");
        AddPL(359, "Hełm Szkieletowy Shell", "Hełm uformowany z kości.");
        AddPL(360, "Hełm Śmierci Shell", "Hełm samej śmierci.");
        AddPL(361, "Strażnik Szkieletowy Nokferatu Shell", "Hełm z kości wampira.");
        AddPL(362, "Hełm Demoniczny Shell", "Legendarny hełm Władcy Demonów.");
        AddPL(363, "Szata Pogrzebowa Shell", "Szata z grobu.");
        AddPL(364, "Stary Płaszcz Shell", "Stary płaszcz z dawnych czasów.");
        AddPL(365, "Płaszcz Kości Nokferatu Shell", "Płaszcz utkany z kości.");
        AddPL(366, "Płaszcz Dusz Shell", "Płaszcz niespokojnych dusz.");
        AddPL(367, "Szata Śmierci Shell", "Szata śmierci.");
        AddPL(368, "Szata Podziemnego Świata Shell", "Szata z głębin.");
        AddPL(369, "Zbroja Demoniczna Shell", "Legendarna zbroja Władcy Demonów.");
        AddPL(370, "Zniszczona Spódnica Shell", "Zniszczona, ale użyteczna spódnica.");
        AddPL(371, "Spódnica z Mutacyjnych Kości Shell", "Spódnica wykonana z mutacyjnych kości.");
        AddPL(372, "Owijacze Cierniowe Nokferatu Shell", "Owijacze z kolcami.");
        AddPL(373, "Strażnik Ciała Nokferatu Shell", "Ochrona wykonana z zakonserwowanego ciała.");
        AddPL(374, "Nagolenniki Krwawe Shell", "Nagolenniki przesiąknięte krwią.");
        AddPL(375, "Krwawy Wędrowiec Nokferatu Shell", "Nagolenniki chodzące w krwi.");
        AddPL(376, "Nagolenniki Demoniczne Shell", "Legendarne nagolenniki Władcy Demonów.");
        AddPL(377, "Nagolenniki Pancerne", "Ciężkie metalowe buty miażdżące wszystko pod stopami. Obrona to najlepszy atak.");
        AddPL(378, "Buty Żeglarza", "Solidne buty odporne na każdą burzę. Kotwiczą Shell w prądach bitwy.");
        AddPL(379, "Nagolenniki Głębin", "Buty wykute w głębinach oceanu. Odporne na wszelkie ciśnienie.");
        AddPL(380, "Buty Miażdżące Kości", "Buty wzmocnione kośćmi potworów. Każdy krok to groźba.");
        AddPL(381, "Buty Twierdzy Magmy", "Buty wykute w ogniu wulkanu. Emitują ciepło palące atakujących.");
        AddPL(382, "Buty Depta Krwi", "Brutalne buty pozostawiające ślady zniszczenia. Wrogowie boją się z nimi walczyć.");
        AddPL(383, "Buty Nieugiętego Strażnika", "Legendarne buty niezniszczalnego obrońcy. Shell staje się nieporuszalną fortecą.");
        
        // Vesper Armor (384-411)
        AddPL(384, "Czapka Czarownicy Vesper", "Prosta czapka dla tkaczy snów.");
        AddPL(385, "Kaptur Dziwny Vesper", "Kaptur szepczący sekrety.");
        AddPL(386, "Kaptur Dziwny Vesper", "Kaptur pulsujący dziwną energią.");
        AddPL(387, "Kaptur Dziwny Vesper", "Kaptur energii koszmarów.");
        AddPL(388, "Owijacze Rozpaczy Vesper", "Owijacze żywiące się rozpaczą.");
        AddPL(389, "Korona Ciemnego Czarownika Vesper", "Korona ciemnej magii.");
        AddPL(390, "Czapka Ferunbrasa Vesper", "Legendarna czapka Mistrza Ciemności.");
        AddPL(391, "Czerwona Szata Vesper", "Szata dla aspirujących magów snów.");
        AddPL(392, "Więzy Dusz Vesper", "Więzy prowadzące dusze.");
        AddPL(393, "Szata Energii Vesper", "Szata trzaskająca energią.");
        AddPL(394, "Spódnica Duchowa Vesper", "Spódnica utkana z energii duchów.");
        AddPL(395, "Płaszcz Dusz Vesper", "Płaszcz zawierający dusze.");
        AddPL(396, "Owijacze Dusz Vesper", "Owijacze uwięzionych dusz.");
        AddPL(397, "Szata Arkanum Smoka Vesper", "Legendarna szata Arkanum Smoka.");
        AddPL(398, "Nagolenniki Dusz Vesper", "Nagolenniki nasycone energią dusz.");
        AddPL(399, "Nagolenniki Egzotyczne Vesper", "Egzotyczne nagolenniki z odległych dziedzin.");
        AddPL(400, "Nagolenniki Dusz Vesper", "Nagolenniki prowadzące siłę dusz.");
        AddPL(401, "Spodnie Krwawe Vesper", "Spodnie zabarwione magią krwi.");
        AddPL(402, "Nagolenniki Magmy Vesper", "Nagolenniki wykute w magicznym ogniu.");
        AddPL(403, "Nagolenniki Mądrości Vesper", "Nagolenniki starożytnej mądrości.");
        AddPL(404, "Spodnie Starożytnych Vesper", "Legendarne spodnie od Starożytnych.");
        AddPL(405, "Pantofle Acolyte", "Miękkie pantofle noszone przez nowych członków świątyni. Niosą modlitwy w każdym kroku.");
        AddPL(406, "Buty Świątynne", "Tradycyjne buty sług El. Kotwiczą Vesper w jej wierze.");
        AddPL(407, "Buty Dziwnego Mnicha", "Buty noszone przez mnichów badających zakazane teksty. Wiedza i wiara się splatają.");
        AddPL(408, "Owijacze Błogosławione przez Krasnoludy", "Owijacze zaklęte przez krasnoludzkich rzemieślników. Technologia spotyka się z boskością.");
        AddPL(409, "Pantofle z Jedwabiu Wampira", "Eleganckie pantofle utkane z jedwabiu wampira. Czerpią życie z samej ziemi.");
        AddPL(410, "Pantofle Zabójcy Demonów", "Zielone pantofle pobłogosławione, aby niszczyć zło. Palą demony w każdym kroku.");
        AddPL(411, "Buty Wypędzające Koszmary", "Legendarne buty przekraczające koszmary, aby ratować niewinnych. Ostateczna misja Vesper.");
        
        // Yubar Armor (412-439)
        AddPL(412, "Maska Plemienna Yubara", "Maska dla wojowników plemiennych.");
        AddPL(413, "Hełm Wikingów Yubara", "Hełm dla wojowników północy.");
        AddPL(414, "Hełm Rogaty Yubara", "Hełm z przerażającymi rogami.");
        AddPL(415, "Nakrycie Głowy Wytrzymałego Ix Yubara", "Nakrycie głowy dla plemienia Ix.");
        AddPL(416, "Nakrycie Głowy Strachu Ognia Yubara", "Nakrycie głowy płonące strachem.");
        AddPL(417, "Hełm Wytrzymałego Ix Yubara", "Hełm dla wojowników Ix.");
        AddPL(418, "Twarz Apokalipsy Yubara", "Legendarna twarz Apokalipsy.");
        AddPL(419, "Skóra Niedźwiedzia Yubara", "Zbroja z potężnego niedźwiedzia.");
        AddPL(420, "Płaszcz Futra Mamuta Yubara", "Płaszcz z mamuta.");
        AddPL(421, "Szata Wytrzymałego Ix Yubara", "Szata dla plemienia Ix.");
        AddPL(422, "Płaszcz Magmy Yubara", "Płaszcz wykuty w magmie.");
        AddPL(423, "Napierśnik Wytrzymałego Ix Yubara", "Napierśnik dla wojowników Ix.");
        AddPL(424, "Płytowa Zbroja Lawy Yubara", "Płytowa zbroja z lawy.");
        AddPL(425, "Zbroja Ognistego Giganta Yubara", "Legendarna zbroja Ognistego Giganta.");
        AddPL(426, "Nagolenniki Wytrzymałego Ix Yubara", "Nagolenniki dla plemienia Ix.");
        AddPL(427, "Spodnie z Mutacyjnej Skóry Yubara", "Spodnie wykonane z mutacyjnej skóry.");
        AddPL(428, "Spódnica Wytrzymałego Ix Yubara", "Spódnica dla wojowników Ix.");
        AddPL(429, "Nagolenniki Krasnoludzkie Yubara", "Solidne nagolenniki krasnoludzkie.");
        AddPL(430, "Nagolenniki Stopu Yubara", "Nagolenniki wzmocnione stopem.");
        AddPL(431, "Nagolenniki Płytowe Yubara", "Ciężkie nagolenniki płytowe.");
        AddPL(432, "Nagolenniki Krasnoludzkie Yubara", "Legendarne nagolenniki krasnoludzkiego rzemiosła.");
        AddPL(433, "Buty Bojowe Tymczasowe", "Buty złożone z fragmentów pola bitwy. Yubar radzi sobie z tym, co znajdzie.");
        AddPL(434, "Owijacze Plemienne", "Tradycyjne owijacze ludu Yubara. Łączą go z przodkami.");
        AddPL(435, "Nagolenniki Wojownika Jelenia", "Buty ozdobione rogami jelenia. Dają moc Króla Lasu.");
        AddPL(436, "Buty Depta Kłów", "Brutalne buty z kolcami w kształcie kłów. Depta wrogów pod stopami.");
        AddPL(437, "Buty Krwawego Berserkera", "Czerwone buty pobudzające wściekłość Yubara. Ból staje się siłą.");
        AddPL(438, "Deptacz Dusz", "Buty zbierające dusze padłych wrogów. Ich moc rośnie z każdą bitwą.");
        AddPL(439, "Buty Mistrza Powrotu do Domu", "Legendarne buty duchowo zabierające Yubara do domu. Jego przodkowie walczą u jego boku.");
        
        // Legendary Items (440-443)
        AddPL(440, "Hełm Skrzydlaty Wędrowca Snów", "Legendarny hełm dający wędrowcom snów zdolność latania. Automatycznie atakuje najbliższych wrogów.");
        AddPL(441, "Płytowa Zbroja Magii Snów", "Legendarna zbroja chroniąca wędrowców snów wszystkich dziedzin. Automatycznie celuje w najbliższych wrogów.");
        AddPL(442, "Nagolenniki Głębin", "Legendarne nagolenniki wykute w najgłębszych częściach snów.");
        AddPL(443, "Buty Chodzenia po Wodzie", "Legendarne buty pozwalające noszącemu chodzić po wodzie. Skarb poszukiwany przez wędrowców snów wszystkich dziedzin.");
        
        // Consumables (444-447)
        AddPL(444, "Tonik Marzyciela", "Proste lekarstwo z ziół ze świata jawy. Nawet najsłabszy sen zaczyna się od kropli nadziei.");
        AddPL(445, "Esencja Marzenia", "Destylowana ze wspomnień spokojnego snu. Ci, którzy ją piją, czują ciepło zapomnianych snów obmywające ich rany.");
        AddPL(446, "Jasna Żywotność", "Stworzona przez Smoothie z pyłu gwiezdnego i fragmentów koszmarów. Płyn lśni światłem tysiąca śpiących gwiazd.");
        AddPL(447, "Eliksir El", "Święty napój pobłogosławiony światłem samego El. Zakon Vesper pilnie strzeże przepisu, gdyż może uleczyć nawet rany zadane przez najciemniejsze koszmary.");
        
        // Universal Legendary Belt (448)
        AddPL(448, "Pas Nieskończonych Snów", "Legendarny pas wiążący noszącego z wiecznym cyklem snów. Jego moc rośnie z każdym pokonanym koszmarem.");
        
        // Hero-Specific Legendary Belts (449-457)
        AddPL(449, "Pas Tkacza Życia Aureny", "Pas wykuty z kradzionych badań nad siłą życiową Aureny. Tka życie i śmierć w doskonałą równowagę.");
        AddPL(450, "Kryształowy Pas Bismuth", "Pas z czystego kryształu pulsujący wizją niewidomej dziewczyny. Widzi to, czego oczy nie mogą.");
        AddPL(451, "Pas Karmazynowego Oka Lacerty", "Pas kanalizujący legendarne spojrzenie Lacerty. Żaden cel nie ucieknie przed jego czujną mocą.");
        AddPL(452, "Pas Mistrza Pojedynków Mist", "Najwyższy pas Domu Astrid. Honor Mist jest utkany w każdej nici.");
        AddPL(453, "Pas Drzewa Świata Nachii", "Pas wyhodowany z samego Drzewa Świata. Wściekłość natury przez niego przepływa.");
        AddPL(454, "Niezniszczalny Łańcuch Huska", "Pas wykuty z niezniszczalnej determinacji. Determinacja Huska jest jego siłą.");
        AddPL(455, "Pas Cienistego Zabójcy Shell", "Pas doskonałej ciemności. Handlery Shell nigdy nie mogły złamać jego woli.");
        AddPL(456, "Pas Zmierzchowego Inkwizytora Vesper", "Najwyższy pas Zakonu El. Sądzi koszmary i wypędza ciemność.");
        AddPL(457, "Pas Przodków Yubara", "Pas przywołujący przodków Yubara. Ich siła przepływa przez każde włókno.");
        
        // Universal Legendary Ring (458)
        AddPL(458, "Pierścień Wiecznych Snów", "Legendarny pierścień łączący noszącego z nieskończoną krainą snów. Jego moc przekracza wszystkie granice.");
        
        // Hero-Specific Legendary Rings (459-467)
        AddPL(459, "Pierścień Wielkiego Mędrca Aureny", "Pierścień Wielkiego Mędrca Arkanum Księżycowego. Aurena wzięła go jako dowód ich obłudy.");
        AddPL(460, "Pierścień Sercowy Bismuth", "Pierścień czystej krystalicznej magii. Pulsuje przekształconą esencją niewidomej dziewczyny.");
        AddPL(461, "Pierścień Królewskiego Egzekutora Lacerty", "Pierścień elitarnego egzekutora Królewskiej Straży. Lacerta zdobył go przez niezliczone bitwy.");
        AddPL(462, "Pierścień Dziedzictwa Astrid Mist", "Pierścień przodków Domu Astrid. Krew Mist przepływa przez jego metal.");
        AddPL(463, "Pierścień Strażnika Lasu Nachii", "Pierścień pobłogosławiony przez duchy lasu Nachii. Moc natury jest jego esencją.");
        AddPL(464, "Pierścień Nieugiętej Woli Huska", "Pierścień wykuty z niezniszczalnej woli. Determinacja Huska jest jego mocą.");
        AddPL(465, "Pierścień Doskonałego Cienia Shell", "Pierścień absolutnej ciemności. Handlery Shell nigdy nie mogły kontrolować jego mocy.");
        AddPL(466, "Pierścień Świętego Płomienia Vesper", "Pierścień zawierający czystą esencję Świętego Płomienia El. Wypędza całą ciemność.");
        AddPL(467, "Pierścień Plemiennego Mistrza Yubara", "Pierścień kanalizujący siłę przodków Yubara. Ich wściekłość wzmacnia każdy cios.");
        
        // Universal Legendary Amulet (468)
        AddPL(468, "Amulet Nieskończonych Snów", "Legendarny amulet wiążący noszącego z wiecznym cyklem snów. Jego moc przekracza wszystkie granice.");
        
        // Hero-Specific Legendary Amulets (469-477)
        AddPL(469, "Amulet Tkacza Życia Aureny", "Amulet wykuty z kradzionych badań nad siłą życiową Aureny. Tka życie i śmierć w doskonałą równowagę.");
        AddPL(470, "Kryształowe Serce Bismuth", "Amulet z czystego kryształu pulsujący wizją niewidomej dziewczyny. Widzi to, czego oczy nie mogą.");
        AddPL(471, "Amulet Karmazynowego Oka Lacerty", "Amulet kanalizujący legendarne spojrzenie Lacerty. Żaden cel nie ucieknie przed jego czujną mocą.");
        AddPL(472, "Amulet Mistrza Pojedynków Mist", "Najwyższy amulet Domu Astrid. Honor Mist jest utkany w każdej gwieździe.");
        AddPL(473, "Amulet Drzewa Świata Nachii", "Amulet wyhodowany z samego Drzewa Świata. Wściekłość natury przez niego przepływa.");
        AddPL(474, "Niezniszczalny Amulet Huska", "Amulet wykuty z niezniszczalnej determinacji. Determinacja Huska jest jego siłą.");
        AddPL(475, "Amulet Cienistego Zabójcy Shell", "Amulet doskonałej ciemności. Handlery Shell nigdy nie mogły złamać jego woli.");
        AddPL(476, "Amulet Zmierzchowego Inkwizytora Vesper", "Najwyższy amulet Zakonu El. Sądzi koszmary i wypędza ciemność.");
        AddPL(477, "Amulet Przodków Yubara", "Amulet przywołujący przodków Yubara. Ich siła przepływa przez każdą gwiazdę.");
    }
    
    private static void AddPL(int id, string name, string desc)
    {
        _namePL[id] = name;
        _descPL[id] = desc;
    }
    
    // ============================================================
    // GERMAN TRANSLATIONS
    // ============================================================
    private static void InitializeDE()
    {
        _nameDE = new Dictionary<int, string>();
        _descDE = new Dictionary<int, string>();
        
        // Aurena Weapons (1-18)
        AddDE(1, "Klaue des Novizen", "Einfache Klauen, die neuen Mitgliedern der Mond-Arkanum-Gesellschaft gegeben wurden. Aurena behielt ihre auch nach ihrer Verbannung.");
        AddDE(2, "Klaue des Gelehrten", "Klauen, die an verbotenen Texten geschärft wurden. Die Kratzer, die sie hinterlassen, flüstern Geheimnisse.");
        AddDE(3, "Mondlicht-Klauen", "Klauen, die schwach im Mondlicht leuchten. Sie kanalisieren Mondenergie in verheerende Schläge.");
        AddDE(4, "Arkanum-Rasiermesser", "Klauen mit Heilungsrunen beschriftet - jetzt zum Reißen statt zum Heilen verwendet.");
        AddDE(5, "Klaue des Verbannten", "Im Geheimen nach ihrer Verbannung geschmiedet. Jeder Hieb erinnert daran, was die Gesellschaft ihr nahm.");
        AddDE(6, "Griff des Ketzers", "Klauen, die einst einem anderen verbannten Weisen gehörten. Ihr Geist führt Aurenas Schläge.");
        AddDE(7, "Opfer-Klauen", "Klauen, die schärfer werden, wenn Aurenas Lebenskraft schwindet. Die Gesellschaft hielt diese Forschung für zu gefährlich.");
        AddDE(8, "Frostgebundene Klauen", "Klauen, die mit Eis aus dem tiefsten Tresor der Gesellschaft gefroren wurden. Ihre Berührung lähmt Fleisch und Geist.");
        AddDE(9, "Mond-Ripper", "Mit verbotenen Verzauberungen verstärkt. Jede Wunde, die sie verursachen, heilt Aurenas Verbündete.");
        AddDE(10, "Strahlende Klauen", "Klauen, die von gestohlenem Sonnenlicht gesegnet wurden. Aurenas Forschung über Lichtmagie erzürnte sowohl Sonnen- als auch Mondkulte.");
        AddDE(11, "Glut-Klauen", "Klauen, die mit Lebenskraft von willigen Träumern entzündet wurden. Die Flammen erlöschen nie.");
        AddDE(12, "Schatten-Klauen", "Klauen, die in der Essenz von Alpträumen getaucht wurden. Aurena lernte, dass Dunkelheit heilt, was Licht nicht kann.");
        AddDE(13, "Gletscher-Schnitter", "Klauen, die aus ewigem Eis geschnitzt wurden. Sie gefrieren das Blut der Feinde und bewahren Aurenas Lebenskraft.");
        AddDE(14, "Phönix-Klauen", "Klauen, die in Phönixflammen geschmiedet und mit Aurenas eigenem Blut gehärtet wurden. Sie brennen mit unauslöschlichem Zorn.");
        AddDE(15, "Leere-Ripper", "Klauen, die die Realität selbst zerreißen können. Die Gesellschaft fürchtet, was Aurena als Nächstes entdecken wird.");
        AddDE(16, "Himmlische Klauen", "Klauen, die mit reinem Sternenlicht durchdrungen wurden. Jeder Schlag ist ein Gebet um Wissen, das die Götter verbergen wollen.");
        AddDE(17, "Klaue des Großweisen", "Zeremonielle Klauen des Großweisen der Gesellschaft. Aurena nutzt sie als Beweis ihrer Heuchelei.");
        AddDE(18, "Klaue des Lebenswebers", "Aurenas Meisterwerk - Klauen, die Lebenskraft stehlen können, ohne sie zu zerstören. Die Gesellschaft nennt es Ketzerei.");
        
        // Bismuth Weapons (19-36)
        AddDE(19, "Leeres Grimoire", "Ein leeres Zauberbuch, das darauf wartet, ausgefüllt zu werden. Wie Bismuth selbst enthalten seine Seiten unendliches Potenzial.");
        AddDE(20, "Buch des Lehrlings", "Ein Zauberbuch für Anfänger. Das blinde Mädchen, das zu Bismuth wurde, zeichnete seine Seiten mit den Fingern nach und träumte von Magie.");
        AddDE(21, "Kristall-Kodex", "Ein Buch, dessen Seiten aus dünnen Edelsteinen bestehen. Es schwingt mit Bismuths kristalliner Natur.");
        AddDE(22, "Tagebuch des Wanderers", "Ein Reisetagebuch, das Beobachtungen des blinden Mädchens aufzeichnet, das nie sah, aber immer wusste.");
        AddDE(23, "Einführung in Edelsteine", "Ein Grundlagenführer zur Kristallmagie. Seine Worte funkeln wie Bismuths regenbogenfarbener Körper.");
        AddDE(24, "Klassiker des Blinden", "Ein Buch, das in erhabenen Buchstaben für Blinde geschrieben wurde. Bismuth liest es durch Berührung und Erinnerung.");
        AddDE(25, "Buch der Glutworte", "Ein Buch, dessen Seiten ewig brennen. Die Flammen sprechen mit Bismuth in Farben, die sie fühlt, nicht sieht.");
        AddDE(26, "Eisiges Wörterbuch", "Ein Buch, das in ewigem Eis eingewickelt ist. Seine eisigen Seiten bewahren Wissen aus der Zeit vor den Träumen.");
        AddDE(27, "Strahlendes Manuskript", "Ein Buch, das inneres Licht ausstrahlt. Bevor sie zu Bismuth wurde, führte es das blinde Mädchen durch die Dunkelheit.");
        AddDE(28, "Schatten-Grimoire", "Ein Buch, das Licht absorbiert. Seine dunklen Seiten enthalten Geheimnisse, die selbst Bismuth zu lesen fürchtet.");
        AddDE(29, "Lebendiges Zauberbuch", "Ein bewusstes Buch, das sich entschied, mit dem blinden Mädchen zu verschmelzen. Zusammen wurden sie etwas Größeres.");
        AddDE(30, "Prisma-Kodex", "Ein Buch, das Magie in ihre Bestandteile zerlegt. Bismuth sieht die Welt durch das Licht, das es bricht.");
        AddDE(31, "Chronik der Leere", "Ein Buch, das die Erinnerungen derer aufzeichnet, die in der Dunkelheit verloren gingen. Bismuth liest ihre Geschichten, um sie zu ehren.");
        AddDE(32, "Grimoire des Edelsteinherzens", "Das ursprüngliche Zauberbuch, das sich mit dem blinden Mädchen verschmolz. Seine Seiten pulsieren mit kristallinem Leben.");
        AddDE(33, "Buch des Blendenden Lichts", "Ein Buch, das so blendend ist, dass es Sehende blenden kann. Für Bismuth ist es nur Wärme.");
        AddDE(34, "Jahrbuch des Frostes", "Ein uraltes Buch, das in der Zeit eingefroren ist. Seine Prophezeiungen erzählen von einem Edelstein, der wie ein Mädchen geht.");
        AddDE(35, "Buch ohne Namen", "Ein Buch ohne Namen, wie das Mädchen ohne Sehkraft. Zusammen fanden sie ihre Identität.");
        AddDE(36, "Klassiker der Hölle", "Ein Buch, das mit der Leidenschaft eines Träumers brennt, der sich weigert, sich von Blindheit definieren zu lassen.");
        
        // Lacerta Weapons (37-54)
        AddDE(37, "Rekruten-Gewehr", "Standardausgabe für Rekruten der Königlichen Garde. Lacerta beherrschte es vor Ende der ersten Trainingswoche.");
        AddDE(38, "Jäger-Langgewehr", "Ein zuverlässiges Jagdgewehr. Lacerta verwendete ein ähnliches, um seine Familie zu ernähren, bevor er der Garde beitrat.");
        AddDE(39, "Patrouillen-Karabiner", "Kompakt und zuverlässig. Perfekt für lange Patrouillen an den Grenzen des Königreichs.");
        AddDE(40, "Gewehr des Scharfschützen", "Ein ausgewogenes Gewehr für diejenigen, die Präzision über Macht schätzen.");
        AddDE(41, "Schwarzpulver-Musket", "Alt, aber zuverlässig. Der Geruch von Schwarzpulver erinnert Lacerta an seine erste Jagd.");
        AddDE(42, "Aufklärungs-Repeater", "Ein leichtes Gewehr für Aufklärungsmissionen. Geschwindigkeit und Tarnung übertreffen rohe Macht.");
        AddDE(43, "Königliches Garde-Gewehr", "Die Waffe, die Lacerta während seines Dienstes trug. Jede Kratzer erzählt eine Geschichte.");
        AddDE(44, "Stolz des Scharfschützen", "Ein präzises Gewehr für diejenigen, die nie verfehlen. Lacertas scharlachrotes Auge sieht, was andere nicht können.");
        AddDE(45, "Langes Flammen-Gewehr", "Geladen mit Brandgeschossen. Feinde brennen lange nach dem Treffer.");
        AddDE(46, "Frostbiss-Karabiner", "Schießt Geschosse, die auf den absoluten Nullpunkt abgekühlt wurden. Ziele verlangsamen sich zu Schneckentempo vor dem tödlichen Schlag.");
        AddDE(47, "Nacht-Gewehr", "Eine Waffe zum Jagen in der Dunkelheit. Seine Schüsse sind leise wie Schatten.");
        AddDE(48, "Dämmerungs-Musket", "Vom Morgenlicht gesegnet. Seine Schüsse durchdringen Dunkelheit und Täuschung.");
        AddDE(49, "Scharlachrotes Auge", "Benannt nach Lacertas legendärem Blick. Kein Ziel entgeht dem Blick dieses Gewehrs.");
        AddDE(50, "Königlicher Henker", "Für die Elite der Garde reserviert. Lacerta erhielt dieses Gewehr nach der Rettung des Königreichs.");
        AddDE(51, "Sonnenfeuer-Kanone", "Gewehre, die konzentriertes Licht abfeuern. Jeder Schuss ist ein Miniatur-Sonnenaufgang.");
        AddDE(52, "Leere-Jäger", "Gewehre, die rein dunkle Kugeln abfeuern. Ziele verschwinden spurlos.");
        AddDE(53, "Absoluter Nullpunkt", "Der kälteste Punkt der Existenz. Sogar die Zeit gefriert davor.");
        AddDE(54, "Drachenatem", "Gewehre, die mit explosiven Brandgeschossen geladen sind. Die Feinde des Königreichs lernten, ihren Donner zu fürchten.");
        
        // Mist Weapons (55-70)
        AddDE(55, "Fecht-Florett", "Ein Trainingsschwert aus Mists Jugend. Selbst damals übertraf sie jeden Ausbilder in Astrid.");
        AddDE(56, "Adeliger Degen", "Ein Degen, der zu Mists adliger Abstammung passt. Leicht, elegant, tödlich.");
        AddDE(57, "Duellanten-Degen", "Ein Degen für Eins-gegen-Eins-Kämpfe. Mist verlor nie ein Duell.");
        AddDE(58, "Schneller Degen", "Leicht wie eine Verlängerung von Mists Arm.");
        AddDE(59, "Astrid-Degen", "In den besten Schmieden Astrids geschmiedet. Ein Symbol für königliches Handwerk.");
        AddDE(60, "Tänzer-Degen", "Ein Degen für diejenigen, die Kampf als Kunst betrachten. Mists Bewegungen sind poetisch.");
        AddDE(61, "Flammen-Degen", "Ein Degen, der von Flammen umgeben ist. Mists Zorn ist so heiß wie ihre Entschlossenheit.");
        AddDE(62, "Schatten-Degen", "Ein Degen, der aus der Dunkelheit zuschlägt. Feinde fallen, bevor sie Mist sich bewegen sehen.");
        AddDE(63, "Frostbiss-Degen", "Ein Degen aus gefrorenem Stahl. Seine Berührung lähmt Körper und Geist.");
        AddDE(64, "Camillas Gnade", "Ein Geschenk von jemandem, den Mist schätzte. Sie kämpft, um ihre Erinnerung zu ehren.");
        AddDE(65, "Parade-Degen", "Ein Degen für Verteidigung und Angriff. Mist verwandelt jeden Angriff in eine Gelegenheit.");
        AddDE(66, "Strahlender Degen", "Ein Degen, der inneres Licht ausstrahlt. Er durchdringt Lügen und Schatten.");
        AddDE(67, "Höllen-Degen", "Ein Degen, der mit unauslöschlichem Eifer brennt. Mists Entschlossenheit kann nicht gelöscht werden.");
        AddDE(68, "Mitternachts-Degen", "Ein Degen, der in absoluter Dunkelheit geschmiedet wurde. Er schlägt mit der Stille des Todes zu.");
        AddDE(69, "Winter-Degen", "Ein Degen, der aus ewigem Eis geschnitzt wurde. Seine Kälte kann nur mit Mists Konzentration im Kampf mithalten.");
        AddDE(70, "Unbeugsamer Degen", "Der legendäre Degen des größten Duellanten von Astrid. Mist erwarb diesen Titel durch unzählige Siege.");
        
        // Nachia Weapons (71-88)
        AddDE(71, "Setzling-Stab", "Ein junger Zweig aus dem Geisterwald. Sogar Setzlinge antworten auf Nachias Ruf.");
        AddDE(72, "Geisterrufer", "Ein Stab, der geschnitzt wurde, um die Stimmen der Waldgeister zu leiten. Sie flüstern Nachia Geheimnisse zu.");
        AddDE(73, "Waldzweig", "Ein lebender Zweig, der noch wächst. Die Magie des Waldes fließt durch ihn.");
        AddDE(74, "Wildnis-Stab", "Wild und unvorhersehbar, wie Nachia selbst. Die Geister lieben seine chaotische Energie.");
        AddDE(75, "Wächter-Stab", "Getragen von denen, die den Wald schützen. Nachia wurde mit dieser Verantwortung geboren.");
        AddDE(76, "Geisterwelt-Rute", "Eine Rute, die die materielle und die Geisterwelt verbindet. Nachia wandelt zwischen beiden.");
        AddDE(77, "Frostholz-Stab", "Ein Stab, der im ewigen Winter gefroren ist. Die kalten Geister des Nordens antworten auf seinen Ruf.");
        AddDE(78, "Schattenwurzel-Stab", "Wächst in den tiefsten Teilen des Waldes. Schattengeister tanzen um ihn.");
        AddDE(79, "Glutholz-Stab", "Ein Stab, der nie aufhört zu brennen. Feuergeister werden von seiner Wärme angezogen.");
        AddDE(80, "Sonnensegen-Stab", "Von Morgendämmerungsgeistern gesegnet. Sein Licht führt verlorene Seelen nach Hause.");
        AddDE(81, "Fenrirs Zähne", "Ein Stab mit Geisterwolfzähnen an der Spitze. Nachias treuer Gefährte leitet seine Macht.");
        AddDE(82, "Alteiche-Stab", "Aus einer tausendjährigen Eiche geschnitzt. Die ältesten Geister erinnern sich daran, als sie gepflanzt wurde.");
        AddDE(83, "Zweig des Weltenbaums", "Ein Zweig vom legendären Weltenbaum. Alle Waldgeister verbeugen sich vor ihm.");
        AddDE(84, "Stab des Geisterkönigs", "Der Stab des Geisterkönigs selbst. Nachia gewann ihn, indem sie den Wald schützte.");
        AddDE(85, "Ewiger Frost-Stab", "Ein Stab aus ewigem Eis aus dem gefrorenen Herzen des Waldes. Wintergeister gehorchen seinen Befehlen.");
        AddDE(86, "Phönix-Rastplatz", "Ein Stab, auf dem Feuergeister nisten. Seine Flammen bringen Wiedergeburt, nicht Zerstörung.");
        AddDE(87, "Strahlender Hain-Stab", "Ein Stab, der das Licht tausend Glühwürmchen ausstrahlt. Hoffnungsgeister tanzen darin.");
        AddDE(88, "Stab des Leere-Wächters", "Ein Stab, der die Grenzen der Welt bewacht. Schattengeister schützen seinen Träger.");
        
        // Shell Weapons (89-106)
        AddDE(89, "Novizen-Katana", "Ein grundlegendes Samurai-Schwert für diejenigen, die den Weg des Schwertes lernen. Shell beherrschte es in wenigen Tagen.");
        AddDE(90, "Stilles Katana", "Ein Samurai-Schwert, das so konstruiert ist, dass es keinen Laut macht. Shells Opfer hören den Tod nie kommen.");
        AddDE(91, "Attentäter-Katana", "Schmal und präzise. Shell braucht nur einen Schlag an der richtigen Stelle.");
        AddDE(92, "Tötungs-Katana", "Effizient. Praktisch. Gnadenlos. Wie Shell selbst.");
        AddDE(93, "Jäger-Katana", "Ein Samurai-Schwert für die Jagd. Shell hört nicht auf, bis das Ziel eliminiert ist.");
        AddDE(94, "Leeres Katana", "Benannt nach der Leere, die Shell fühlt. Oder nicht fühlt.");
        AddDE(95, "Henker-Katana", "Ein Samurai-Schwert, das unzählige Leben beendet hat. Shell fühlt nichts für irgendeines von ihnen.");
        AddDE(96, "Präzisions-Katana", "Ein Samurai-Schwert, das darauf abgestimmt ist, Schwächen auszunutzen. Shell berechnet den optimalen Angriff.");
        AddDE(97, "Frostbiss-Katana", "Ein Samurai-Schwert kalter Bosheit. Seine Kälte passt zu Shells leerem Herzen.");
        AddDE(98, "Reinigungs-Katana", "Ein helles Samurai-Schwert für dunkle Arbeit. Shell empfindet keine Ironie.");
        AddDE(99, "Schatten-Katana", "Ein Samurai-Schwert, das Schatten trinkt. Shell bewegt sich unsichtbar, schlägt geräuschlos zu.");
        AddDE(100, "Glühender Dolch", "Ein Samurai-Schwert, das von innerem Feuer erhitzt wurde. Shells Ziele brennen, bevor sie bluten.");
        AddDE(101, "Werkzeug des perfekten Mörders", "Das ultimative Werkzeug des ultimativen Mörders. Shell wurde für dieses Schwert geboren.");
        AddDE(102, "Gnadenlose Klinge", "Ein Samurai-Schwert, das sich nie abstumpft, geschwungen von einem unaufhörlichen Verfolger.");
        AddDE(103, "Höllen-Attentäter", "Ein Samurai-Schwert, das in Höllenfeuer geschmiedet wurde. Es brennt mit der Intensität von Shells einzigen Zweck.");
        AddDE(104, "Leere-Katana", "Ein Samurai-Schwert absoluter Dunkelheit. Wie Shell existiert es nur, um zu beenden.");
        AddDE(105, "Gefrorenes Katana", "Ein Samurai-Schwert aus ewigem Eis. Sein Urteil ist so kalt und endgültig wie Shell.");
        AddDE(106, "Strahlendes Ende", "Die ultimative Form des hellen Samurai-Schwerts. Shell vollstreckt Urteile im Namen von El.");
        
        // Vesper Weapons (107-124)
        AddDE(107, "Novizen-Zepter", "Ein einfaches Zepter für neue Mitglieder des Sonnenflammen-Ordens. Vesper begann hier ihre Reise.");
        AddDE(108, "Hammer des Urteils", "Ein Kriegshammer, der verwendet wird, um Els Willen zu vollstrecken. Jeder Schlag ist ein heiliges Urteil.");
        AddDE(109, "Stab des Feuerwächters", "Eine Waffe, die von denen getragen wird, die das heilige Feuer bewachen. Seine Schläge reinigen Unreinheit.");
        AddDE(110, "Akolyt-Schild", "Ein einfacher Schild für Akolythen des Ordens. Glaube ist die beste Verteidigung.");
        AddDE(111, "Inquisitor-Schild", "Ein Schild, der von Inquisitoren getragen wird. Er hat den Fall unzähliger Ketzer gesehen.");
        AddDE(112, "Dämmerungs-Verteidiger", "Ein Schild, der zwischen Licht und Dunkelheit bewacht.");
        AddDE(113, "Fanatik-Hammer", "Ein Kriegshammer, der mit religiösem Fanatismus brennt. Feinde zittern vor ihm.");
        AddDE(114, "Schwerer Reinigungs-Hammer", "Ein riesiger Kriegshammer, der verwendet wird, um das Böse zu reinigen. Ein Schlag zerschmettert Sünde.");
        AddDE(115, "Els Urteil", "Ein Kriegshammer, der persönlich von El gesegnet wurde. Sein Urteil ist unanfechtbar.");
        AddDE(116, "Bastion des Glaubens", "Ein unzerstörbarer Schild. Solange der Glaube nicht erlischt, wird er nicht brechen.");
        AddDE(117, "Heiliger Flammenschild", "Ein Schild, der von heiligem Feuer umgeben ist. Böses, das ihn berührt, wird verbrannt.");
        AddDE(118, "Inquisitions-Bastion", "Der Schild eines hohen Inquisitors. Vesper gewann ihn durch gnadenlosen Dienst.");
        AddDE(119, "Dämmerungs-Zerschmetterer", "Ein legendärer Kriegshammer, der die Dunkelheit zerschmettert. Vesper schwingt die Verkörperung von Els Zorn.");
        AddDE(120, "Heiliger Zerstörer", "Ein Kriegshammer, der vom höchsten Priester gesegnet wurde. Seine Schläge tragen das Gewicht heiligen Zorns.");
        AddDE(121, "Els Rechte Hand", "Die heiligste Waffe des Ordens. Vesper ist das auserwählte Werkzeug von Els Willen.");
        AddDE(122, "Flammen-Festung", "Die ultimative Verteidigung des Ordens. Els Licht schützt Vesper vor allem Schaden.");
        AddDE(123, "Ewige Wachsamkeit", "Ein unerschütterlicher Schild. Wie Vesper wacht er immer über die Dunkelheit.");
        AddDE(124, "Gelübde des Dämmerungs-Paladins", "Der Schild des Ordensführers. Vesper schwor, ihre Seele an El zu binden.");
        
        // Yubar Weapons (125-142)
        AddDE(125, "Neugeborener Stern", "Ein Stern, der sich gerade bildet. Yubar erinnert sich, als alle Sterne so klein waren.");
        AddDE(126, "Kosmische Glut", "Ein Fragment von Sternenfeuer. Es brennt mit der Hitze der Schöpfung selbst.");
        AddDE(127, "Sternenstaub-Kugel", "Komprimierter Sternenstaub, der darauf wartet, geformt zu werden. Yubar webt Galaxien aus solcher Materie.");
        AddDE(128, "Traum-Katalysator", "Eine Kugel, die Traumenergie verstärkt. Stammt aus Yubars ersten Experimenten.");
        AddDE(129, "Nebel-Kern", "Das Herz eines fernen Nebels. Yubar pflückte es aus dem kosmischen Gewebe.");
        AddDE(130, "Himmels-Samen", "Ein Samen, der zu einem Stern werden wird. Yubar züchtet unzählige solcher Samen.");
        AddDE(131, "Supernova-Fragment", "Ein Fragment einer explodierenden Sterns. Yubar war Zeuge seines Todes und bewahrte seine Macht.");
        AddDE(132, "Gravitations-Brunnen", "Eine Kugel, die Gravitation komprimiert. Der Raum selbst krümmt sich um sie.");
        AddDE(133, "Leere-Singularität", "Ein Punkt unendlicher Dichte. Yubar verwendet ihn, um Welten zu erschaffen und zu zerstören.");
        AddDE(134, "Sonnenkorona", "Ein Fragment der äußeren Atmosphäre der Sonne. Es brennt mit dem Zorn des Sterns.");
        AddDE(135, "Gefrorener Komet", "Ein gefangener Kometenkern. Er trägt Geheimnisse aus den Tiefen des Kosmos.");
        AddDE(136, "Sternenschmiede", "Eine Miniatur eines Sternkerns. Yubar verwendet sie, um neue Realität zu schmieden.");
        AddDE(137, "Relikt des Urknalls", "Ein Fragment des Urknalls der Schöpfung. Es enthält den Ursprung des Universums.");
        AddDE(138, "Kosmisches Webstuhl", "Ein Werkzeug, das Realität webt. Yubar verwendet es, um Schicksal umzugestalten.");
        AddDE(139, "Strahlende Schöpfung", "Das Licht der Schöpfung selbst. Yubar verwendete es, um die ersten Sterne zu gebären.");
        AddDE(140, "Herz der Entropie", "Die Essenz des kosmischen Verfalls. Alles endet, und Yubar entscheidet wann.");
        AddDE(141, "Absoluter Nullpunkt", "Der kälteste Punkt der Existenz. Sogar die Zeit gefriert davor.");
        AddDE(142, "Katastrophen-Kern", "Das Herz der kosmischen Zerstörung. Yubar setzt es nur in kritischen Momenten frei.");
        
        // Rings (143-162)
        AddDE(143, "Ring des Träumers", "Ein einfacher Ring, der von denen getragen wird, die gerade in die Welt der Träume eintreten. Er strahlt schwache Hoffnung aus.");
        AddDE(144, "Sternenstaub-Ring", "Aus kondensiertem Sternenstaub geschmiedet. Träumer sagen, er leuchtet heller in Gefahr.");
        AddDE(145, "Schlaf-Marke", "Gravur eines friedlichen Schlafs. Träger erholen sich schneller von Wunden.");
        AddDE(146, "Geister-Ring", "Ein Ring, der umherwandernde Geister einfängt. Ihr Licht führt Wege durch dunkle Träume.");
        AddDE(147, "Ring des Wanderers", "Getragen von Wanderern der Traumreiche. Er hat unzählige vergessene Wege gesehen.");
        AddDE(148, "Alptraum-Fragment-Ring", "Ein Ring, der Fragmente besiegter Alpträume enthält. Seine Dunkelheit stärkt die Mutigen.");
        AddDE(149, "Hüter der Erinnerung", "Ein Ring, der kostbare Erinnerungen bewahrt. Smoothie verkauft ähnliche in seinem Laden.");
        AddDE(150, "Ring des Mond-Arkanums", "Ein Ring aus der Mond-Arkanum-Gesellschaft. Aurena trug einen ähnlichen vor ihrer Verbannung.");
        AddDE(151, "Geisterwald-Marke", "Ein Ring, der von den Geistern von Nachias Wald gesegnet wurde. Die Kraft der Natur fließt durch ihn.");
        AddDE(152, "Abzeichen der Königlichen Garde", "Ein Ring, den Lacerta während seines Dienstes trug. Er trägt immer noch das Gewicht der Verantwortung.");
        AddDE(153, "Ruhm des Duellanten", "Ein Ring, der unter adligen Duellanten weitergegeben wird. Mists Familie schätzt solche Ringe.");
        AddDE(154, "Flammenglut-Amulett", "Eine versiegelte Funke des heiligen Feuers. Vespers Orden bewacht diese Reliquien.");
        AddDE(155, "Zauberbuch-Fragment", "Ein Ring aus Bismuths kristalliner Magie. Er summt vor Arkanum-Energie.");
        AddDE(156, "Jäger-Marke", "Ein Ring, der Beute verfolgt. Shells Kontrolleure verwenden sie, um ihre Werkzeuge zu überwachen.");
        AddDE(157, "Els Segen", "Ein Ring, der von El selbst gesegnet wurde. Sein Licht vertreibt Alpträume und heilt die Verwundeten.");
        AddDE(158, "Alptraum-Lord-Marke", "Von einem besiegten Alptraum-Lord genommen. Seine dunkle Macht korrumpiert und stärkt.");
        AddDE(159, "Astrid-Erbe", "Ein Ring aus dem adligen Haus von Astrid. Mists Vorfahren trugen ihn in unzähligen Duellen.");
        AddDE(160, "Auge des Sehers", "Ein Ring, der durch den Schleier der Träume sehen kann. Vergangenheit, Gegenwart und Zukunft verschwimmen.");
        AddDE(161, "Ring des Ur-Traums", "Im Morgengrauen der Träume geschmiedet. Er enthält die Essenz des ersten Träumers.");
        AddDE(162, "Ewiger Schlaf", "Ein Ring, der den tiefsten Schlaf berührt. Träger fürchten weder Tod noch Erwachen.");
        
        // Amulets (163-183)
        AddDE(163, "Anhänger des Träumers", "Ein einfacher Anhänger, der von denen getragen wird, die gerade in die Welt der Träume eintreten. Er strahlt schwache Hoffnung aus.");
        AddDE(164, "Anhänger der Feuerberührung", "Ein Anhänger, der vom Feuer berührt wurde. Seine Wärme vertreibt Kälte und Angst.");
        AddDE(165, "Scharlachrote Träne", "Ein Rubin in Form einer blutigen Träne. Seine Macht kommt vom Opfer.");
        AddDE(166, "Bernstein-Amulett", "Ein Bernstein, der ein uraltes Wesen enthält. Seine Erinnerungen stärken den Träger.");
        AddDE(167, "Dämmerungs-Medaille", "Eine Medaille, die zwischen Licht und Dunkelheit flackert. Neue Mitglieder von Vesper tragen diese.");
        AddDE(168, "Sternenstaub-Halskette", "Eine Halskette, die mit kristallinen Sternen bestreut ist. Smoothie sammelt sie aus gefallenen Träumen.");
        AddDE(169, "Smaragd-Auge", "Ein grüner Edelstein, der durch Illusionen sehen kann. Alpträume können seinem Blick nicht entgehen.");
        AddDE(170, "Herz des Waldgeistes", "Ein Edelstein, der den Segen der Waldgeister enthält. Nachias Wächter schätzen diese.");
        AddDE(171, "Mond-Arkanum-Marke", "Eine Marke aus verbotenen Archiven. Aurena riskiert alles, um diese zu studieren.");
        AddDE(172, "Bismuth-Kristall", "Ein Fragment kristalliner reiner Magie. Es schwingt mit Arkanum-Energie.");
        AddDE(173, "Flammenglut-Amulett", "Eine versiegelte Funke des heiligen Feuers. Vespers Orden bewacht diese Reliquien.");
        AddDE(174, "Alptraum-Zahn-Anhänger", "Ein Zahn von einem mächtigen Alptraum, jetzt eine Trophäe. Shell trägt einen als Erinnerung.");
        AddDE(175, "Medaille der Königlichen Garde", "Eine Ehrenmedaille aus der Königlichen Garde. Lacerta gewann viele vor seiner Verbannung.");
        AddDE(176, "Astrid-Wappen", "Das adlige Wappen der Familie Astrid. Mists Familienerbe hängt an dieser Kette.");
        AddDE(177, "Els Heilige Träne", "Eine Träne, die von El selbst vergossen wurde. Sein Licht schützt vor den dunkelsten Alpträumen.");
        AddDE(178, "Ur-Smaragd", "Ein Edelstein aus dem ersten Wald. Er enthält die Träume uralter Geister.");
        AddDE(179, "Auge des Alptraum-Lords", "Das Auge eines besiegten Alptraum-Lords. Es sieht alle Ängste und nutzt sie.");
        AddDE(180, "Herz der Flamme", "Der Kern des heiligen Feuers selbst. Nur die Frommsten können es tragen.");
        AddDE(181, "Orakel des Sehers", "Ein Amulett, das alle möglichen Zukünfte sehen kann. Realität beugt sich vor seinen Prophezeiungen.");
        AddDE(182, "Kompass des Leere-Wanderers", "Ein Amulett, das auf die Leere zeigt. Diejenigen, die ihm folgen, kehren nie auf die gleiche Weise zurück.");
        AddDE(183, "Midas-Amulett", "Ein legendäres Amulett, das vom Traum des Goldes berührt wurde. Besiegte Feinde lassen zusätzliches Gold fallen.");
        AddDE(478, "Sternenstaub-Sammler", "Ein legendäres Amulett, das kristallisierten Sternenstaub von besiegten Feinden sammelt. Jeder Kill liefert kostbare Traumesenz (Basis 1-5, skaliert mit Aufwertungsstufe und Monsterstärke).");
        
        // Belts (184-203)
        AddDE(184, "Gürtel des Wanderers", "Ein einfacher Stoffgürtel, der von denen getragen wird, die zwischen Träumen wandern. Er verankert die Seele.");
        AddDE(185, "Seil des Träumers", "Ein Seil, das aus Traumfäden gewebt ist. Es pulsiert sanft mit jedem Herzschlag.");
        AddDE(186, "Sternenstaub-Gürtel", "Ein Gürtel, der mit kristallinem Sternenstaub verziert ist. Smoothie verkauft ähnliche Accessoires.");
        AddDE(187, "Wächter-Gürtel", "Ein solider Gürtel, der von Wächtern der Träume getragen wird. Er lockert sich nie, versagt nie.");
        AddDE(188, "Mystischer Gürtel", "Ein Gürtel, der mit schwacher Magie durchdrungen ist. Er sticht, wenn Alpträume nahen.");
        AddDE(189, "Jäger-Gürtel", "Ein Ledergürtel mit Versorgungstaschen. Unerlässlich für lange Reisen in Träumen.");
        AddDE(190, "Pilger-Gürtel", "Getragen von denen, die Els Licht suchen. Er bietet Trost in den dunkelsten Träumen.");
        AddDE(191, "Alptraum-Jäger-Gürtel", "Ein Gürtel aus besiegten Alpträumen. Ihre Essenz stärkt den Träger.");
        AddDE(192, "Arkanum-Gürtel", "Ein Gürtel aus der Mond-Arkanum-Gesellschaft. Er verstärkt magische Energie.");
        AddDE(193, "Flammen-Gürtel", "Ein Gürtel, der vom Orden Vesper gesegnet wurde. Er brennt mit Schutzfeuer.");
        AddDE(194, "Duellanten-Schwertgürtel", "Ein eleganter Gürtel, der von adligen Kriegern von Astrid getragen wird. Er trägt Schwerter und Ehre.");
        AddDE(195, "Seil des Geisterwebers", "Ein Gürtel, der von Waldgeistern für Nachia gewebt wurde. Er summt vor Natur-Energie.");
        AddDE(196, "Werkzeug-Gürtel des Attentäters", "Ein Gürtel mit versteckten Fächern. Shells Kontrolleure verwenden sie, um ihre Werkzeuge auszurüsten.");
        AddDE(197, "Revolver-Holster-Gürtel", "Ein Gürtel, der für Lacertas Feuerwaffen entworfen wurde. Schnelles Ziehen, schnelleres Töten.");
        AddDE(198, "Els Heiliger Gürtel", "Ein Gürtel, der von El selbst gesegnet wurde. Sein Licht verankert die Seele gegen alle Dunkelheit.");
        AddDE(199, "Kette des Alptraum-Lords", "Ein Gürtel, der aus den Ketten eines Alptraum-Lords geschmiedet wurde. Er bindet Angst an den Willen des Trägers.");
        AddDE(200, "Gürtel des Ur-Traums", "Ein Gürtel aus dem ersten Traum. Er enthält das Echo der Schöpfung selbst.");
        AddDE(201, "Gürtel der Weltenbaum-Wurzel", "Ein Gürtel, der aus den Wurzeln des Weltenbaums wächst. Nachias Wald segnete seine Schöpfung.");
        AddDE(202, "Astrid-Erbstück", "Der Familiengürtel der Familie Astrid. Mists Blut ist in sein Gewebe gewebt.");
        AddDE(203, "Dämmerungs-Inquisitor-Gürtel", "Vespers Gürtel höchster Stufe. Er richtet alle, die vor ihm stehen.");
        
        // Armor Sets - Bone Sentinel (204-215)
        AddDE(204, "Maske des Knochen-Wächters", "Eine Maske, die aus dem Schädel eines gefallenen Alptraums geschnitzt wurde. Ihre leeren Augen sehen durch Illusionen.");
        AddDE(205, "Helm des Geisterwelt-Wächters", "Ein Helm, der mit Geistern von Nachias Wald durchdrungen ist. Flüstern des Schutzes umgeben den Träger.");
        AddDE(206, "Krone des Alptraum-Lords", "Eine Krone, die aus eroberten Alpträumen geschmiedet wurde. Der Träger beherrscht die Angst selbst.");
        AddDE(207, "Weste des Knochen-Wächters", "Ein Rüstung aus den Rippen von Wesen, die in Träumen starben. Ihr Schutz dauert über den Tod hinaus.");
        AddDE(208, "Brustpanzer des Geisterwelt-Wächters", "Ein Brustpanzer, der aus kristallisierten Träumen gewebt ist. Er bewegt sich und passt sich ankommenden Schlägen an.");
        AddDE(209, "Brustpanzer des Alptraum-Lords", "Eine Rüstung, die mit dunkler Energie pulsiert. Alpträume verbeugen sich vor ihrem Träger.");
        AddDE(210, "Beinschienen des Knochen-Wächters", "Beinschienen, die mit Alptraum-Knochen verstärkt sind. Sie werden nie müde, geben nie nach.");
        AddDE(211, "Beinschienen des Geisterwelt-Wächters", "Beinschienen, die von Waldgeistern gesegnet wurden. Der Träger bewegt sich wie Wind durch Blätter.");
        AddDE(212, "Beinschienen des Alptraum-Lords", "Beinschienen, die Schatten trinken. Dunkelheit stärkt jeden Schritt.");
        AddDE(213, "Stiefel des Knochen-Wächters", "Stiefel zum lautlosen Gehen in den Traumreichen. Die Toten machen keinen Lärm.");
        AddDE(214, "Eisenstiefel des Geisterwelt-Wächters", "Stiefel, die keine Spuren in den Traumreichen hinterlassen. Perfekt für Alptraum-Jäger.");
        AddDE(215, "Stiefel des Alptraum-Lords", "Stiefel, die Welten überschreiten. Realität beugt sich unter jedem Schritt.");
        
        // Aurena Armor (216-243)
        AddDE(216, "Aurenas Lehrlings-Hut", "Ein einfacher Hut, der von Novizen des Mond-Arkanums getragen wird.");
        AddDE(217, "Aurenas mystischer Turban", "Ein Turban, der mit schwacher Mondlicht-Energie durchdrungen ist.");
        AddDE(218, "Aurenas verführerische Krone", "Eine Krone, die die magische Affinität des Trägers verstärkt.");
        AddDE(219, "Aurenas alte Krone", "Eine alte Reliquie, die durch Generationen weitergegeben wurde.");
        AddDE(220, "Aurenas Falken-Kopfband", "Ein Kopfband, das von Himmelsgeistern gesegnet wurde.");
        AddDE(221, "Aurenas Macht-Krone", "Eine mächtige Krone, die Arkanum-Macht ausstrahlt.");
        AddDE(222, "Aurenas Mondlicht-Krone", "Die legendäre Krone eines Meisters des Mond-Arkanums.");
        AddDE(223, "Aurenas blaue Robe", "Eine einfache Robe, die von Mondmagiern getragen wird.");
        AddDE(224, "Aurenas Magier-Robe", "Eine Robe, die von praktizierenden Magiern geschätzt wird.");
        AddDE(225, "Aurenas verzauberte Robe", "Eine Robe, die mit magischen Fäden gewebt ist.");
        AddDE(226, "Aurenas erleuchtende Robe", "Eine Robe, die Mondweisheit leitet.");
        AddDE(227, "Aurenas Traum-Gewand", "Ein Gewand, das aus kristallisierten Träumen gewebt ist.");
        AddDE(228, "Aurenas königliche Schuppen-Robe", "Eine Robe, die für die Mondkönigsfamilie geeignet ist.");
        AddDE(229, "Aurenas Eiskönigin-Robe", "Die legendäre Robe der Eiskönigin selbst.");
        AddDE(230, "Aurenas blaue Beinschienen", "Einfache Beinschienen, die von aspirierenden Magiern getragen werden.");
        AddDE(231, "Aurenas Faser-Rock", "Ein leichter Rock, der freie Bewegung ermöglicht.");
        AddDE(232, "Aurenas elfenhafte Beinschienen", "Eleganter Beinschienen, die von Elfen entworfen wurden.");
        AddDE(233, "Aurenas erleuchtende Beinschienen", "Beinschienen, die von Weisheit gesegnet wurden.");
        AddDE(234, "Aurenas Gletscher-Rock", "Ein Rock, der mit Frostmagie durchdrungen ist.");
        AddDE(235, "Aurenas Frost-Hosen", "Hosen, die Kältekraft ausstrahlen.");
        AddDE(236, "Aurenas prächtige Beinschienen", "Legendäre Beinschienen unvergleichlicher Eleganz.");
        AddDE(237, "Weiche Mondlicht-Pantoffeln", "Weiche Schuhe, die von Akolythen des Mond-Arkanums getragen werden. Sie polstern jeden Schritt mit Mondlicht.");
        AddDE(238, "Pilger-Sandalen", "Einfache Sandalen, die von den Ältesten der Gesellschaft gesegnet wurden. Aurena erinnert sich noch an ihre Wärme.");
        AddDE(239, "Schuhe östlicher Mystik", "Exotische Schuhe aus fernen Ländern. Sie leiten heilende Energie durch die Erde.");
        AddDE(240, "Gnade des Traumwanderers", "Stiefel, die zwischen der wachen und der Traumwelt wandeln. Perfekt für Heiler, die verlorene Seelen suchen.");
        AddDE(241, "Stiefel des Seelenwanderers", "Stiefel, die mit der Essenz verstorbener Heiler durchdrungen sind. Ihre Weisheit leitet jeden Schritt.");
        AddDE(242, "Stiefel des Seelenverfolgers", "Dunkle Stiefel, die die Verwundeten verfolgen. Aurena verwendet sie, um diejenigen zu finden, die Hilfe brauchen... oder die sie verletzt haben.");
        AddDE(243, "Flügel des gefallenen Weisen", "Legendäre Stiefel, die angeblich reinen Herzen die Fähigkeit zu fliegen verleihen. Aurenas Verbannung bewies ihre Qualifikation.");
        
        // Bismuth Armor (244-271)
        AddDE(244, "Bismuths Kegelhut", "Einfacher Hut für Kristall-Lehrlinge.");
        AddDE(245, "Bismuths Smaragd-Hut", "Hut mit Smaragd-Kristallen verziert.");
        AddDE(246, "Bismuths Gletscher-Maske", "Maske aus gefrorenen Kristallen.");
        AddDE(247, "Bismuths Alarharim-Maske", "Alte Maske kristalliner Macht.");
        AddDE(248, "Bismuths Prisma-Helm", "Helm, der Licht in Regenbogen bricht.");
        AddDE(249, "Bismuths reflektierende Krone", "Krone, die magische Energie reflektiert.");
        AddDE(250, "Bismuths weißglühende Krone", "Legendäre Krone, die mit Kristallfeuer brennt.");
        AddDE(251, "Bismuths Eis-Robe", "Robe, die von Kristallmagie gekühlt wurde.");
        AddDE(252, "Bismuths Mönchs-Robe", "Einfache Robe für Meditation.");
        AddDE(253, "Bismuths Gletscher-Robe", "Robe, die in ewigem Frost gefroren ist.");
        AddDE(254, "Bismuths Kristall-Rüstung", "Rüstung, die aus lebenden Kristallen geformt wurde.");
        AddDE(255, "Bismuths Prisma-Rüstung", "Rüstung, die das Licht selbst krümmt.");
        AddDE(256, "Bismuths gefrorene Plattenrüstung", "Plattenrüstung aus ewigem Frost.");
        AddDE(257, "Bismuths heilige Plattenrüstung", "Legendäre Rüstung, die von kristallinen Göttern gesegnet wurde.");
        AddDE(258, "Bismuths Kettenhemd-Beinschienen", "Grundlegende Beinschienen mit kristallinen Gliedern.");
        AddDE(259, "Bismuths Faser-Beinschienen", "Leichte Beinschienen für Kristallmagier.");
        AddDE(260, "Bismuths Smaragd-Beinschienen", "Beinschienen mit Smaragd-Kristallen verziert.");
        AddDE(261, "Bismuths seltsame Hosen", "Hosen, die mit seltsamer Energie pulsieren.");
        AddDE(262, "Bismuths Prisma-Beinschienen", "Beinschienen, die mit Prisma-Licht funkeln.");
        AddDE(263, "Bismuths Alarharim-Beinschienen", "Alte Beinschienen von Alarharim.");
        AddDE(264, "Bismuths Falken-Beinschienen", "Legendäre Beinschienen des Falken-Ordens.");
        AddDE(265, "Gelehrten-Pantoffeln", "Bequeme Schuhe für lange Arbeit in Bibliotheken. Wissen fließt durch ihre Sohlen.");
        AddDE(266, "Alarharim-Owijacze", "Alte Owijacze aus verlorener Zivilisation. Sie flüstern vergessene Zaubersprüche.");
        AddDE(267, "Eis-Schuhe", "Stiefel aus ewigem Eis geschnitzt. Bismuth hinterlässt Frostmuster, wo sie geht.");
        AddDE(268, "Frostblüten-Schritt", "Eleganter Stiefel, der Eiskristalle blüht. Jeder Schritt schafft einen Frostgarten.");
        AddDE(269, "Kristall-Resonanz-Stiefel", "Stiefel, die magische Energie verstärken. Kristalle summen vor ungenutzter Macht.");
        AddDE(270, "Prisma-Arkanum-Stiefel", "Stiefel, die Licht in reine magische Energie bricht. Realität beugt sich um jeden Schritt.");
        AddDE(271, "Leere-Wanderer-Stiefel", "Legendäre Stiefel, die die Leere selbst überschreiten. Bismuth sieht Wege, die andere sich nicht vorstellen können.");
        
        // Lacerta Armor (272-299)
        AddDE(272, "Echsenmenschen Leder-Helm", "Grundlegender Helm für Drachenkrieger.");
        AddDE(273, "Echsenmenschen Kettenhemd-Helm", "Solider Kettenhemd-Helm.");
        AddDE(274, "Echsenmenschen Krieger-Helm", "Helm, der von erfahrenen Kriegern getragen wird.");
        AddDE(275, "Echsenmenschen Drachenschuppen-Helm", "Helm aus Drachenschuppen geschmiedet.");
        AddDE(276, "Echsenmenschen königlicher Helm", "Helm, der für die königliche Drachenfamilie geeignet ist.");
        AddDE(277, "Echsenmenschen Elite-Drachenmensch-Helm", "Elite-Helm des Drachenmensch-Ordens.");
        AddDE(278, "Echsenmenschen goldener Helm", "Legendärer goldener Helm des Drachenkönigs.");
        AddDE(279, "Echsenmenschen Leder-Rüstung", "Grundlegende Rüstung für Drachenkrieger.");
        AddDE(280, "Echsenmenschen Schuppen-Rüstung", "Rüstung, die mit Schuppen verstärkt ist.");
        AddDE(281, "Echsenmenschen Ritter-Rüstung", "Schwere Rüstung für Drachenritter.");
        AddDE(282, "Echsenmenschen Drachenschuppen-Kettenhemd", "Kettenhemd aus Drachenschuppen geschmiedet.");
        AddDE(283, "Echsenmenschen königliches Drachenmensch-Kettenhemd", "Königliches Kettenhemd des Drachenmensch-Ordens.");
        AddDE(284, "Echsenmenschen Falken-Plattenrüstung", "Plattenrüstung des Falken-Ordens.");
        AddDE(285, "Echsenmenschen goldene Rüstung", "Legendäre goldene Rüstung des Drachenkönigs.");
        AddDE(286, "Echsenmenschen Leder-Beinschienen", "Grundlegende Beinschienen für Drachenkrieger.");
        AddDE(287, "Echsenmenschen genietete Beinschienen", "Beinschienen, die mit Nieten verstärkt sind.");
        AddDE(288, "Echsenmenschen Ritter-Beinschienen", "Schwere Beinschienen für Drachenritter.");
        AddDE(289, "Echsenmenschen Drachenschuppen-Beinschienen", "Beinschienen aus Drachenschuppen geschmiedet.");
        AddDE(290, "Echsenmenschen königliche Beinschienen", "Beinschienen, die für die königliche Familie geeignet sind.");
        AddDE(291, "Echsenmenschen prächtige Beinschienen", "Prachtvolle Beinschienen meisterhaften Handwerks.");
        AddDE(292, "Echsenmenschen goldene Beinschienen", "Legendäre goldene Beinschienen des Drachenkönigs.");
        AddDE(293, "Abgenutzte Lederstiefel", "Einfache Stiefel, die viele Schlachten gesehen haben. Sie tragen den Duft des Abenteuers.");
        AddDE(294, "Geflickte Kampfstiefel", "Stiefel, die unzählige Male geflickt wurden. Jeder Flicken erzählt eine Überlebensgeschichte.");
        AddDE(295, "Stiefel des Stahlkriegers", "Schwere Stiefel für den Kampf geschmiedet. Sie verankern Lacerta am Boden während des Kampfes.");
        AddDE(296, "Wächter-Beinschienen", "Stiefel, die von Elite-Wächtern getragen werden. Sie weichen nie zurück, ergeben sich nie.");
        AddDE(297, "Drachenschuppen-Beinschienen", "Stiefel aus Drachenschuppen gemacht. Sie verleihen dem Träger die Zähigkeit eines Drachen.");
        AddDE(298, "Stiefel des Drachenmensch-Kriegsherrn", "Stiefel alter Drachenmensch-Generäle. Furchterregend auf jedem Schlachtfeld.");
        AddDE(299, "Stiefel des goldenen Meisters", "Legendäre Stiefel des größten Kriegers einer Ära. Lacertas Schicksal wartet.");
        
        // Mist Armor (300-327)
        AddDE(300, "Mists Turban", "Einfacher Turban, der die Identität verbirgt.");
        AddDE(301, "Mists leichter Turban", "Leichter, atmungsaktiver Turban.");
        AddDE(302, "Mists Nachtsicht-Turban", "Turban, der Nachtsicht verstärkt.");
        AddDE(303, "Mists Blitz-Kopfband", "Kopfband, das mit elektrischer Energie geladen ist.");
        AddDE(304, "Mists Kobra-Kapuze", "Kapuze, die tödlich wie eine Schlange ist.");
        AddDE(305, "Mists Höllen-Verfolger-Maske", "Maske, die Beute verfolgt.");
        AddDE(306, "Mists dunkler Flüsterer", "Legendäre Maske, die Tod flüstert.");
        AddDE(307, "Mists Leder-Geschirr", "Leichtes Geschirr für Beweglichkeit.");
        AddDE(308, "Mists Waldläufer-Umhang", "Umhang für schnelle Bewegung.");
        AddDE(309, "Mists Mitternachts-Tunika", "Tunika für nächtliche Schläge.");
        AddDE(310, "Mists Blitz-Robe", "Robe, die mit Blitzen geladen ist.");
        AddDE(311, "Mists Spannungs-Rüstung", "Rüstung, die mit elektrischer Energie pulsiert.");
        AddDE(312, "Mists Meister-Bogenschützen-Rüstung", "Rüstung eines Bogenmeisters.");
        AddDE(313, "Mists dunkler Lord-Umhang", "Legendärer Umhang des Dunklen Lords.");
        AddDE(314, "Mists Waldläufer-Beinschienen", "Beinschienen, die von Waldläufern getragen werden.");
        AddDE(315, "Mists Dschungel-Überlebens-Beinschienen", "Beinschienen für Dschungel-Überlebende.");
        AddDE(316, "Mists Mitternachts-Sarong", "Sarong für nächtliche Operationen.");
        AddDE(317, "Mists Blitz-Beinschienen", "Beinschienen, die mit Blitzen geladen sind.");
        AddDE(318, "Mists Heuschrecken-Beinschienen", "Beinschienen für erstaunliche Sprünge.");
        AddDE(319, "Mists grüner Dämon-Beinschienen", "Beinschienen dämonischer Geschwindigkeit.");
        AddDE(320, "Mists Seelenwanderer-Beinschienen", "Legendäre Beinschienen, die Welten überschreiten.");
        AddDE(321, "Temporäre Aufklärungsstiefel", "Stiefel aus Fragmenten zusammengesetzt. Leicht und leise, perfekt für Aufklärung.");
        AddDE(322, "Sumpf-Läufer-Stiefel", "Wasserdichte Stiefel für schwieriges Gelände. Mist kennt jeden Abkürzung.");
        AddDE(323, "Schneller Wanderer", "Verzauberte Stiefel, die die Schritte des Trägers beschleunigen. Perfekt für Treffer-und-Lauf-Taktik.");
        AddDE(324, "Hirsch-Jäger-Stiefel", "Stiefel, die Beute verfolgen.");
        AddDE(325, "Blitz-Schlag-Stiefel", "Stiefel, die mit Blitz-Energie geladen sind.");
        AddDE(326, "Feuer-Wanderer-Kampfstiefel", "Stiefel, die durch Feuer gehen können.");
        AddDE(327, "Leiden-Treter", "Legendäre Stiefel, die alles Leiden zertreten.");
        
        // Nachia Armor (328-355)
        AddDE(328, "Nachias Pelz-Hut", "Einfacher Hut für Waldjäger.");
        AddDE(329, "Nachias Feder-Kopfschmuck", "Kopfschmuck mit Federn verziert.");
        AddDE(330, "Nachias Schamanen-Maske", "Maske geistiger Macht.");
        AddDE(331, "Nachias Erde-Kapuze", "Kapuze, die von Erdgeistern gesegnet wurde.");
        AddDE(332, "Nachias Natur-Helm", "Helm, der mit Naturkraft durchdrungen ist.");
        AddDE(333, "Nachias Baum-Krone", "Krone, die aus einem alten Baum wächst.");
        AddDE(334, "Nachias Blatt-Krone", "Legendäre Krone des Waldwächters.");
        AddDE(335, "Nachias Pelz-Rüstung", "Rüstung aus Waldwesen gemacht.");
        AddDE(336, "Nachias einheimische Rüstung", "Traditionelle Rüstung der Waldstämme.");
        AddDE(337, "Nachias grünes Holz-Mantel", "Mantel, der aus lebenden Ranken gewebt ist.");
        AddDE(338, "Nachias Erde-Umhang", "Umhang, der von Erdgeistern gesegnet wurde.");
        AddDE(339, "Nachias Natur-Umarmung", "Rüstung, die mit der Natur vereint ist.");
        AddDE(340, "Nachias Sumpf-Nest-Rüstung", "Rüstung aus tiefen Sümpfen.");
        AddDE(341, "Nachias Blatt-Robe", "Legendäre Robe des Waldwächters.");
        AddDE(342, "Nachias Mammut-Pelz-Shorts", "Warme Shorts aus Mammut-Pelz gemacht.");
        AddDE(343, "Nachias Leder-Shorts", "Traditionelle Jagd-Beinschienen.");
        AddDE(344, "Nachias Hirsch-Beinschienen", "Beinschienen aus Hirschhaut gemacht.");
        AddDE(345, "Nachias Erde-Beinschienen", "Beinschienen, die von Erdgeistern gesegnet wurden.");
        AddDE(346, "Nachias Blatt-Beinschienen", "Beinschienen, die aus magischen Blättern gewebt sind.");
        AddDE(347, "Nachias Wildschweinmensch-Lendenschurz", "Lendenschurz von einem mächtigen Wildschweinmensch.");
        AddDE(348, "Nachias blutige Jagd-Beinschienen", "Legendäre Beinschienen der blutigen Jagd.");
        AddDE(349, "Pelz-gefütterte Stiefel", "Warme, bequeme Stiefel.");
        AddDE(350, "Dachs-Haut-Stiefel", "Stiefel aus Dachs-Haut gemacht.");
        AddDE(351, "Kobra-Schlag-Stiefel", "Stiefel, die schnell wie eine Schlange sind.");
        AddDE(352, "Wald-Schleicher-Owijacze", "Owijacze, die im Wald lautlos sind.");
        AddDE(353, "Erde-Jäger-Stiefel", "Stiefel, die Beute verfolgen.");
        AddDE(354, "Fieberblüten-Waldläufer-Stiefel", "Stiefel mit Fieberblüten verziert. Sie verleihen fieberhafte Geschwindigkeit und tödliche Präzision.");
        AddDE(355, "Blut-Räuber-Stiefel", "Legendäre Stiefel, die mit dem Blut unzähliger Jagden getränkt sind. Nachia wurde zum Spitzen-Räuber.");
        
        // Shell Armor (356-383)
        AddDE(356, "Shells beschädigter Helm", "Beschädigter Helm aus der Unterwelt.");
        AddDE(357, "Shells gebrochene Maske", "Gebrochene Maske, die immer noch Schutz bietet.");
        AddDE(358, "Shells Knochen-Lord-Helm", "Helm aus den Überresten des Knochen-Lords geschmiedet.");
        AddDE(359, "Shells Skelett-Helm", "Helm aus Knochen geformt.");
        AddDE(360, "Shells Todes-Helm", "Helm des Todes selbst.");
        AddDE(361, "Shells Nokferatu-Skelett-Wächter", "Helm aus Vampir-Knochen.");
        AddDE(362, "Shells Dämonen-Helm", "Legendärer Helm des Dämonen-Lords.");
        AddDE(363, "Shells Begräbnis-Gewand", "Gewand aus dem Grab.");
        AddDE(364, "Shells alter Umhang", "Alter Umhang aus alten Zeiten.");
        AddDE(365, "Shells Nokferatu-Knochen-Umhang", "Umhang, der aus Knochen gewebt ist.");
        AddDE(366, "Shells Seelen-Umhang", "Umhang unruhiger Seelen.");
        AddDE(367, "Shells Todes-Robe", "Robe des Todes.");
        AddDE(368, "Shells Unterwelt-Robe", "Robe aus den Tiefen.");
        AddDE(369, "Shells Dämonen-Rüstung", "Legendäre Rüstung des Dämonen-Lords.");
        AddDE(370, "Shells beschädigte Schürze", "Beschädigte, aber nützliche Schürze.");
        AddDE(371, "Shells mutierte Knochen-Schürze", "Schürze aus mutierten Knochen gemacht.");
        AddDE(372, "Shells Nokferatu-Dornen-Owijacze", "Owijacze mit Dornen.");
        AddDE(373, "Shells Nokferatu-Fleisch-Wächter", "Schutz aus konserviertem Fleisch gemacht.");
        AddDE(374, "Shells blutige Beinschienen", "Beinschienen, die mit Blut getränkt sind.");
        AddDE(375, "Shells Nokferatu-Blut-Wanderer", "Beinschienen, die im Blut wandeln.");
        AddDE(376, "Shells Dämonen-Beinschienen", "Legendäre Beinschienen des Dämonen-Lords.");
        AddDE(377, "Panzer-Beinschienen", "Schwere Metallstiefel, die alles unter den Füßen zermalmen. Verteidigung ist der beste Angriff.");
        AddDE(378, "Seefahrer-Stiefel", "Solide Stiefel, die jedem Sturm widerstehen. Sie verankern Shell in den Strömungen des Kampfes.");
        AddDE(379, "Tiefen-Beinschienen", "Stiefel, die in den Tiefen des Ozeans geschmiedet wurden. Widerstandsfähig gegen jeden Druck.");
        AddDE(380, "Knochen-Zermalm-Stiefel", "Stiefel, die mit Monster-Knochen verstärkt sind. Jeder Schritt ist eine Bedrohung.");
        AddDE(381, "Magma-Festung-Stiefel", "Stiefel, die im Feuer des Vulkans geschmiedet wurden. Sie strahlen Hitze aus, die Angreifer verbrennt.");
        AddDE(382, "Blut-Treter-Stiefel", "Brutale Stiefel, die Spuren der Zerstörung hinterlassen. Feinde fürchten sich davor, gegen sie zu kämpfen.");
        AddDE(383, "Stiefel des unerschütterlichen Wächters", "Legendäre Stiefel eines unzerstörbaren Verteidigers. Shell wird zu einer unbeweglichen Festung.");
        
        // Vesper Armor (384-411)
        AddDE(384, "Vespers Hexen-Hut", "Einfacher Hut für Traumweber.");
        AddDE(385, "Vespers seltsame Kapuze", "Kapuze, die Geheimnisse flüstert.");
        AddDE(386, "Vespers seltsame Kapuze", "Kapuze, die mit seltsamer Energie pulsiert.");
        AddDE(387, "Vespers Alptraum-Energie-Kapuze", "Kapuze der Alptraum-Energie.");
        AddDE(388, "Vespers Verzweiflung-Owijacze", "Owijacze, die sich von Verzweiflung ernähren.");
        AddDE(389, "Vespers dunkler Hexer-Krone", "Krone der dunklen Magie.");
        AddDE(390, "Vespers Ferunbras-Hut", "Legendärer Hut des Meisters der Dunkelheit.");
        AddDE(391, "Vespers rote Robe", "Robe für aspirierende Traummagier.");
        AddDE(392, "Vespers Seelen-Fesseln", "Fesseln, die Seelen leiten.");
        AddDE(393, "Vespers Energie-Robe", "Robe, die mit Energie knistert.");
        AddDE(394, "Vespers Geister-Rock", "Rock, der aus Geister-Energie gewebt ist.");
        AddDE(395, "Vespers Seelen-Umhang", "Umhang, der Seelen enthält.");
        AddDE(396, "Vespers Seelen-Owijacze", "Owijacze gefangener Seelen.");
        AddDE(397, "Vespers Arkanum-Drachen-Robe", "Legendäre Robe des Arkanum-Drachen.");
        AddDE(398, "Vespers Seelen-Beinschienen", "Beinschienen, die mit Seelen-Energie durchdrungen sind.");
        AddDE(399, "Vespers exotische Beinschienen", "Exotische Beinschienen aus fernen Reichen.");
        AddDE(400, "Vespers Seelen-Beinschienen", "Beinschienen, die Seelenkraft leiten.");
        AddDE(401, "Vespers blutige Hosen", "Hosen, die mit Blutmagie gefärbt sind.");
        AddDE(402, "Vespers Magma-Beinschienen", "Beinschienen, die in magischem Feuer geschmiedet wurden.");
        AddDE(403, "Vespers Weisheits-Beinschienen", "Beinschienen alter Weisheit.");
        AddDE(404, "Vespers Alte-Hosen", "Legendäre Hosen von den Alten.");
        AddDE(405, "Akolyt-Pantoffeln", "Weiche Pantoffeln, die von neuen Mitgliedern des Tempels getragen werden. Sie tragen Gebete in jedem Schritt.");
        AddDE(406, "Tempel-Schuhe", "Traditionelle Schuhe der Diener von El. Sie verankern Vesper in ihrem Glauben.");
        AddDE(407, "Seltsame Mönchs-Stiefel", "Stiefel, die von Mönchen getragen werden, die verbotene Texte studieren. Wissen und Glaube verflechten sich.");
        AddDE(408, "Zwergen-gesegnete Owijacze", "Owijacze, die von Zwergen-Handwerkern verzaubert wurden. Technologie trifft auf Göttlichkeit.");
        AddDE(409, "Vampir-Seiden-Pantoffeln", "Eleganter Pantoffeln aus Vampir-Seide gewebt. Sie ziehen Leben aus der Erde selbst.");
        AddDE(410, "Dämonen-Killer-Pantoffeln", "Grüne Pantoffeln, die gesegnet wurden, um das Böse zu zerstören. Sie verbrennen Dämonen in jedem Schritt.");
        AddDE(411, "Alptraum-Vertreiber-Stiefel", "Legendäre Stiefel, die Alpträume überschreiten, um Unschuldige zu retten. Vespers ultimative Mission.");
        
        // Yubar Armor (412-439)
        AddDE(412, "Yubars Stammes-Maske", "Maske für Stammeskrieger.");
        AddDE(413, "Yubars Wikinger-Helm", "Helm für nördliche Krieger.");
        AddDE(414, "Yubars gehörnter Helm", "Helm mit furchterregenden Hörnern.");
        AddDE(415, "Yubars ausdauernder Ix-Kopfschmuck", "Kopfschmuck für den Ix-Stamm.");
        AddDE(416, "Yubars Feuer-Angst-Kopfschmuck", "Kopfschmuck, der mit Angst brennt.");
        AddDE(417, "Yubars ausdauernder Ix-Helm", "Helm für Ix-Krieger.");
        AddDE(418, "Yubars Apokalypse-Gesicht", "Legendäres Gesicht der Apokalypse.");
        AddDE(419, "Yubars Bären-Pelz", "Rüstung aus einem mächtigen Bären.");
        AddDE(420, "Yubars Mammut-Pelz-Umhang", "Umhang aus einem Mammut.");
        AddDE(421, "Yubars ausdauernder Ix-Robe", "Robe für den Ix-Stamm.");
        AddDE(422, "Yubars Magma-Mantel", "Mantel, der in Magma geschmiedet wurde.");
        AddDE(423, "Yubars ausdauernder Ix-Brustpanzer", "Brustpanzer für Ix-Krieger.");
        AddDE(424, "Yubars Lava-Plattenrüstung", "Plattenrüstung aus Lava.");
        AddDE(425, "Yubars Feuer-Gigant-Rüstung", "Legendäre Rüstung des Feuer-Giganten.");
        AddDE(426, "Yubars ausdauernder Ix-Beinschienen", "Beinschienen für den Ix-Stamm.");
        AddDE(427, "Yubars mutierte Haut-Hosen", "Hosen aus mutierter Haut gemacht.");
        AddDE(428, "Yubars ausdauernder Ix-Rock", "Rock für Ix-Krieger.");
        AddDE(429, "Yubars Zwergen-Beinschienen", "Solide Zwergen-Beinschienen.");
        AddDE(430, "Yubars Legierungs-Beinschienen", "Beinschienen, die mit Legierung verstärkt sind.");
        AddDE(431, "Yubars Platten-Beinschienen", "Schwere Platten-Beinschienen.");
        AddDE(432, "Yubars Zwergen-Beinschienen", "Legendäre Beinschienen zwergischen Handwerks.");
        AddDE(433, "Temporäre Kampfstiefel", "Stiefel aus Schlachtfeld-Fragmenten zusammengesetzt. Yubar macht das Beste aus dem, was er findet.");
        AddDE(434, "Stammes-Owijacze", "Traditionelle Owijacze des Volkes von Yubar. Sie verbinden ihn mit seinen Vorfahren.");
        AddDE(435, "Hirsch-Krieger-Beinschienen", "Stiefel mit Hirsch-Geweihen verziert. Sie verleihen die Macht des Königs des Waldes.");
        AddDE(436, "Reißzahn-Treter-Stiefel", "Brutale Stiefel mit Reißzahn-förmigen Stacheln. Sie treten Feinde unter die Füße.");
        AddDE(437, "Blut-Berserker-Stiefel", "Blutrote Stiefel, die Yubars Raserei entfachen. Schmerz wird zu Kraft.");
        AddDE(438, "Seelen-Treter", "Stiefel, die die Seelen gefallener Feinde ernten. Ihre Macht wächst mit jeder Schlacht.");
        AddDE(439, "Stiefel des Heimkehr-Meisters", "Legendäre Stiefel, die Yubar geistig nach Hause bringen. Seine Vorfahren kämpfen an seiner Seite.");
        
        // Legendary Items (440-443)
        AddDE(440, "Flügel-Helm des Traumwanderers", "Legendärer Helm, der Traumwanderern die Fähigkeit zu fliegen verleiht. Greift automatisch die nächsten Feinde an.");
        AddDE(441, "Traum-Magie-Plattenrüstung", "Legendäre Rüstung, die Traumwanderer aller Reiche schützt. Zielt automatisch auf die nächsten Feinde.");
        AddDE(442, "Tiefen-Beinschienen", "Legendäre Beinschienen, die in den tiefsten Teilen der Träume geschmiedet wurden.");
        AddDE(443, "Wasser-Gehen-Stiefel", "Legendäre Stiefel, die dem Träger erlauben, auf Wasser zu gehen. Ein Schatz, der von Traumwanderern aller Reiche gesucht wird.");
        
        // Consumables (444-447)
        AddDE(444, "Träumer-Tonikum", "Ein einfaches Heilmittel aus Kräutern der wachen Welt. Selbst der schwächste Traum beginnt mit einem Tropfen Hoffnung.");
        AddDE(445, "Träumerei-Essenz", "Destilliert aus Erinnerungen an friedlichen Schlummer. Wer es trinkt, spürt die Wärme vergessener Träume über seinen Wunden.");
        AddDE(446, "Luzide Vitalität", "Von Smoothie aus Sternenstaub und Alptraum-Fragmenten hergestellt. Die Flüssigkeit schimmert im Licht tausend schlafender Sterne.");
        AddDE(447, "Elixier von El", "Ein heiliger Trank, gesegnet vom Licht Els selbst. Vespers Orden hütet sein Rezept eifersüchtig, denn es kann selbst Wunden heilen, die von den dunkelsten Alpträumen zugefügt wurden.");
        
        // Universal Legendary Belt (448)
        AddDE(448, "Gürtel der Unendlichen Träume", "Ein legendärer Gürtel, der den Träger an den ewigen Zyklus der Träume bindet. Seine Macht wächst mit jedem besiegten Alptraum.");
        
        // Hero-Specific Legendary Belts (449-457)
        AddDE(449, "Aurenas Lebensweber-Gürtel", "Ein Gürtel, geschmiedet aus Aurenas gestohlenen Forschungen zur Lebenskraft. Er webt Leben und Tod in perfektes Gleichgewicht.");
        AddDE(450, "Bismuths Kristalliner Gurt", "Ein Gürtel aus reinem Kristall, der mit der Vision des blinden Mädchens pulsiert. Er sieht, was Augen nicht können.");
        AddDE(451, "Lacertas Karmesin-Augen-Gürtel", "Ein Gürtel, der Lacertas legendären Blick kanalisiert. Kein Ziel entgeht seiner wachsamen Macht.");
        AddDE(452, "Mists Duellanten-Champion-Gürtel", "Der ultimative Gürtel von Haus Astrid. Mists Ehre ist in jeden Faden gewebt.");
        AddDE(453, "Nachias Weltenbaum-Gürtel", "Ein Gürtel, der vom Weltenbaum selbst gewachsen ist. Die Wut der Natur fließt durch ihn.");
        AddDE(454, "Husks Unzerbrechliche Kette", "Ein Gürtel, geschmiedet aus unzerbrechlichem Entschluss. Husks Entschlossenheit ist seine Stärke.");
        AddDE(455, "Shells Schatten-Assassinen-Gürtel", "Ein Gürtel perfekter Dunkelheit. Shells Handler konnten seinen Willen nie brechen.");
        AddDE(456, "Vespers Zwielicht-Inquisitor-Gürtel", "Der höchste Gürtel von Els Orden. Er richtet Alpträume und bannt Dunkelheit.");
        AddDE(457, "Yubars Ahnen-Champion-Gürtel", "Ein Gürtel, der Yubars Ahnen beschwört. Ihre Stärke fließt durch jede Faser.");
        
        // Universal Legendary Ring (458)
        AddDE(458, "Ring der Ewigen Träume", "Ein legendärer Ring, der den Träger mit dem unendlichen Reich der Träume verbindet. Seine Macht transzendiert alle Grenzen.");
        
        // Hero-Specific Legendary Rings (459-467)
        AddDE(459, "Aurenas Großweisen-Ring", "Der Ring des Großweisen des Mond-Arkanum. Aurena nahm ihn als Beweis ihrer Heuchelei.");
        AddDE(460, "Bismuths Edelsteinherz-Ring", "Ein Ring aus reinem kristallinem Zauber. Er pulsiert mit der transformierten Essenz des blinden Mädchens.");
        AddDE(461, "Lacertas Königlicher Henker-Ring", "Der Ring des Elite-Henkers der Königlichen Garde. Lacerta verdiente ihn durch unzählige Schlachten.");
        AddDE(462, "Mists Astrid-Erbe-Ring", "Der Ahnenring von Haus Astrid. Mists Blutlinie fließt durch sein Metall.");
        AddDE(463, "Nachias Waldwächter-Ring", "Ein Ring, gesegnet von den Geistern von Nachias Wald. Die Macht der Natur ist seine Essenz.");
        AddDE(464, "Husks Unbeugsamer Wille-Ring", "Ein Ring, geschmiedet aus unzerbrechlichem Willen. Husks Entschlossenheit ist seine Macht.");
        AddDE(465, "Shells Perfekter Schatten-Ring", "Ein Ring absoluter Dunkelheit. Shells Handler konnten seine Macht nie kontrollieren.");
        AddDE(466, "Vespers Heiliger Flammen-Ring", "Ein Ring, der die reine Essenz von Els Heiliger Flamme enthält. Er bannt alle Dunkelheit.");
        AddDE(467, "Yubars Stammes-Champion-Ring", "Ein Ring, der die Stärke von Yubars Ahnen kanalisiert. Ihre Wut stärkt jeden Schlag.");
        
        // Universal Legendary Amulet (468)
        AddDE(468, "Amulett der Unendlichen Träume", "Ein legendäres Amulett, das den Träger an den ewigen Zyklus der Träume bindet. Seine Macht transzendiert alle Grenzen.");
        
        // Hero-Specific Legendary Amulets (469-477)
        AddDE(469, "Aurenas Lebensweber-Amulett", "Ein Amulett, geschmiedet aus Aurenas gestohlenen Forschungen zur Lebenskraft. Es webt Leben und Tod in perfektes Gleichgewicht.");
        AddDE(470, "Bismuths Kristallines Herz", "Ein Amulett aus reinem Kristall, das mit der Vision des blinden Mädchens pulsiert. Es sieht, was Augen nicht können.");
        AddDE(471, "Lacertas Karmesin-Augen-Amulett", "Ein Amulett, das Lacertas legendären Blick kanalisiert. Kein Ziel entgeht seiner wachsamen Macht.");
        AddDE(472, "Mists Duellanten-Champion-Amulett", "Das ultimative Amulett von Haus Astrid. Mists Ehre ist in jeden Edelstein gewebt.");
        AddDE(473, "Nachias Weltenbaum-Amulett", "Ein Amulett, das vom Weltenbaum selbst gewachsen ist. Die Wut der Natur fließt durch ihn.");
        AddDE(474, "Husks Unzerbrechliches Amulett", "Ein Amulett, geschmiedet aus unzerbrechlichem Entschluss. Husks Entschlossenheit ist seine Stärke.");
        AddDE(475, "Shells Schatten-Assassinen-Amulett", "Ein Amulett perfekter Dunkelheit. Shells Handler konnten seinen Willen nie brechen.");
        AddDE(476, "Vespers Zwielicht-Inquisitor-Amulett", "Das höchste Amulett von Els Orden. Es richtet Alpträume und bannt Dunkelheit.");
        AddDE(477, "Yubars Ahnen-Champion-Amulett", "Ein Amulett, das Yubars Ahnen beschwört. Ihre Stärke fließt durch jeden Edelstein.");
    }
    
    private static void AddDE(int id, string name, string desc)
    {
        _nameDE[id] = name;
        _descDE[id] = desc;
    }
    
    // ============================================================
    // FRENCH TRANSLATIONS
    // ============================================================
    private static void InitializeFR()
    {
        _nameFR = new Dictionary<int, string>();
        _descFR = new Dictionary<int, string>();
        
        // Aurena Weapons (1-18)
        AddFR(1, "Griffes de l'Initiateur", "Griffes simples données aux nouveaux membres de la Société Arcanum Lunaire. Aurena garda les siennes même après son expulsion.");
        AddFR(2, "Serres du Savant", "Griffes aiguisées sur des textes interdits. Les égratignures qu'elles laissent chuchotent des secrets.");
        AddFR(3, "Griffes Lunaires", "Griffes qui brillent faiblement au clair de lune. Elles canalisent l'énergie lunaire en frappes dévastatrices.");
        AddFR(4, "Rasoirs Arcanum", "Griffes inscrites de runes de guérison - maintenant utilisées pour déchirer plutôt que guérir.");
        AddFR(5, "Griffes de l'Exilé", "Forgées en secret après son bannissement. Chaque entaille rappelle ce que la Société lui a pris.");
        AddFR(6, "Étreinte de l'Hérétique", "Griffes qui appartenaient autrefois à un autre sage expulsé. Leur esprit guide les frappes d'Aurena.");
        AddFR(7, "Griffes Sacrificielles", "Griffes qui deviennent plus tranchantes à mesure que la force vitale d'Aurena décline. La Société jugea cette recherche trop dangereuse.");
        AddFR(8, "Griffes Liées au Givre", "Griffes gelées avec de la glace du coffre le plus profond de la Société. Leur toucher engourdit la chair et l'esprit.");
        AddFR(9, "Déchireurs Lunaires", "Renforcés avec des enchantements interdits. Chaque blessure qu'ils infligent guérit les alliés d'Aurena.");
        AddFR(10, "Griffes Radieuses", "Griffes bénies par la lumière solaire volée. Les recherches d'Aurena sur la magie de la lumière ont enragé les cultes du soleil et de la lune.");
        AddFR(11, "Griffes de Braise", "Griffes enflammées avec la force vitale extraite de rêveurs consentants. Les flammes ne meurent jamais.");
        AddFR(12, "Serres Ombrales", "Griffes trempées dans l'essence des cauchemars. Aurena apprit que l'obscurité guérit ce que la lumière ne peut pas.");
        AddFR(13, "Moissonneur Glaciaire", "Griffes sculptées dans la glace éternelle. Elles gèlent le sang des ennemis et préservent la force vitale d'Aurena.");
        AddFR(14, "Griffes du Phénix", "Griffes forgées dans les flammes du phénix et trempées dans le sang d'Aurena elle-même. Elles brûlent d'une colère inextinguible.");
        AddFR(15, "Déchireur du Vide", "Griffes capables de déchirer la réalité elle-même. La Société craint ce qu'Aurena découvrira ensuite.");
        AddFR(16, "Griffes Célestes", "Griffes imprégnées de pure lumière stellaire. Chaque coup est une prière pour la connaissance que les dieux veulent cacher.");
        AddFR(17, "Griffes du Grand Sage", "Griffes cérémonielles du grand sage de la Société. Aurena les utilise comme preuve de leur hypocrisie.");
        AddFR(18, "Griffes du Tisseur de Vie", "Chef-d'œuvre d'Aurena - griffes capables de voler la force vitale sans la détruire. La Société appelle cela hérésie.");
        
        // Bismuth Weapons (19-36)
        AddFR(19, "Grimoire Vide", "Un livre de sorts vide attendant d'être rempli. Comme Bismuth elle-même, ses pages contiennent un potentiel infini.");
        AddFR(20, "Livre de l'Apprenti", "Un livre de sorts pour débutants. La fille aveugle qui devint Bismuth traçait ses pages du doigt, rêvant de magie.");
        AddFR(21, "Codex Cristallin", "Un livre dont les pages sont faites de gemmes fines. Il résonne avec la nature cristalline de Bismuth.");
        AddFR(22, "Journal du Vagabond", "Un journal de voyage enregistrant les observations de la fille aveugle qui ne vit jamais mais sut toujours.");
        AddFR(23, "Introduction aux Gemmes", "Un guide de base sur la magie cristalline. Ses mots scintillent comme le corps arc-en-ciel de Bismuth.");
        AddFR(24, "Classique de l'Aveugle", "Un livre écrit en lettres en relief pour les aveugles. Bismuth le lit par le toucher et la mémoire.");
        AddFR(25, "Livre des Mots de Braise", "Un livre dont les pages brûlent éternellement. Les flammes parlent à Bismuth en couleurs qu'elle ressent, pas voit.");
        AddFR(26, "Dictionnaire de Glace", "Un livre enveloppé de glace éternelle. Ses pages glacées préservent la connaissance d'avant le début des rêves.");
        AddFR(27, "Manuscrit Radieux", "Un livre émettant une lumière intérieure. Avant qu'elle ne devienne Bismuth, il guida la fille aveugle à travers l'obscurité.");
        AddFR(28, "Grimoire de l'Ombre", "Un livre qui absorbe la lumière. Ses pages sombres contiennent des secrets que même Bismuth craint de lire.");
        AddFR(29, "Livre de Sorts Vivant", "Un livre conscient qui choisit de fusionner avec la fille aveugle. Ensemble, ils devinrent quelque chose de plus grand.");
        AddFR(30, "Codex Prismatique", "Un livre qui décompose la magie en ses couleurs composantes. Bismuth voit le monde à travers la lumière qu'il réfracte.");
        AddFR(31, "Chronique du Vide", "Un livre enregistrant les souvenirs de ceux qui périrent dans l'obscurité. Bismuth lit leurs histoires pour les honorer.");
        AddFR(32, "Grimoire du Cœur de Gemme", "Le livre de sorts original qui fusionna avec la fille aveugle. Ses pages pulsent de vie cristalline.");
        AddFR(33, "Livre de la Lumière Aveuglante", "Un livre si aveuglant qu'il peut aveugler les voyants. Pour Bismuth, ce n'est que chaleur.");
        AddFR(34, "Annales Liées au Givre", "Un livre ancien gelé dans le temps. Ses prophéties racontent une gemme qui marche comme une fille.");
        AddFR(35, "Livre Sans Nom", "Un livre sans nom, comme la fille sans vue. Ensemble, ils trouvèrent leur identité.");
        AddFR(36, "Classique de l'Enfer", "Un livre brûlant de la passion d'un rêveur qui refuse de laisser la cécité le définir.");
        
        // Lacerta Weapons (37-54)
        AddFR(37, "Fusil du Recrue", "Arme standard délivrée aux recrues de la Garde Royale. Lacerta le maîtrisa avant la fin de la première semaine d'entraînement.");
        AddFR(38, "Fusil de Chasse Long", "Un fusil de chasse fiable. Lacerta en utilisa un similaire pour nourrir sa famille avant de rejoindre la Garde.");
        AddFR(39, "Carabine de Patrouille", "Compacte et fiable. Parfaite pour de longues patrouilles aux frontières du royaume.");
        AddFR(40, "Fusil du Tireur d'Élite", "Un fusil équilibré conçu pour ceux qui privilégient la précision sur la puissance.");
        AddFR(41, "Mousquet à Poudre Noire", "Vieux mais fiable. L'odeur de la poudre noire rappelle à Lacerta sa première chasse.");
        AddFR(42, "Carabine d'Éclaireur", "Un fusil léger pour les missions d'éclaireur. La vitesse et la furtivité surpassent la puissance brute.");
        AddFR(43, "Fusil de la Garde Royale", "L'arme que Lacerta porta pendant son service. Chaque égratignure raconte une histoire.");
        AddFR(44, "Fierté du Sniper", "Un fusil précis pour ceux qui ne manquent jamais. L'œil écarlate de Lacerta voit ce que les autres ne peuvent pas.");
        AddFR(45, "Fusil Long Flamboyant", "Chargé avec des balles incendiaires. Les ennemis brûlent longtemps après l'impact.");
        AddFR(46, "Carabine Morsure de Givre", "Tire des balles refroidies au zéro absolu. Les cibles ralentissent à un rythme d'escargot avant le coup fatal.");
        AddFR(47, "Fusil de la Nuit", "Une arme pour chasser dans l'obscurité. Ses tirs sont silencieux comme l'ombre.");
        AddFR(48, "Mousquet de l'Aube", "Béni par la lumière de l'aube. Ses tirs percent l'obscurité et la tromperie.");
        AddFR(49, "Œil Écarlate", "Nommé d'après le regard légendaire de Lacerta. Aucune cible n'échappe au regard de ce fusil.");
        AddFR(50, "Exécuteur Royal", "Réservé à l'élite de la Garde. Lacerta reçut ce fusil après avoir sauvé le royaume.");
        AddFR(51, "Canon Solaire", "Fusils tirant de la lumière concentrée. Chaque tir est un lever de soleil miniature.");
        AddFR(52, "Chasseur du Vide", "Fusils tirant des balles de pure obscurité. Les cibles disparaissent sans trace.");
        AddFR(53, "Zéro Absolu", "Le point le plus froid de l'existence. Même le temps gèle devant lui.");
        AddFR(54, "Souffle du Dragon", "Fusils chargés avec des balles explosives incendiaires. Les ennemis du royaume apprirent à craindre son rugissement.");
        
        // Mist Weapons (55-70)
        AddFR(55, "Fleuret d'Entraînement", "Épée d'entraînement de la jeunesse de Mist. Même alors, elle surpassa chaque instructeur d'Astrid.");
        AddFR(56, "Rapière Noble", "Rapière assortie à la lignée noble de Mist. Légère, élégante, mortelle.");
        AddFR(57, "Rapière de Duelliste", "Rapière conçue pour le combat un contre un. Mist ne perdit jamais un duel.");
        AddFR(58, "Rapière Rapide", "Légère comme une extension du bras de Mist.");
        AddFR(59, "Rapière d'Astrid", "Forgée dans les meilleures forges d'Astrid. Symbole de l'artisanat royal.");
        AddFR(60, "Rapière du Danseur", "Rapière pour ceux qui traitent le combat comme un art. Les mouvements de Mist sont poétiques.");
        AddFR(61, "Rapière Flamboyante", "Rapière entourée de flammes. La colère de Mist est aussi chaude que sa détermination.");
        AddFR(62, "Rapière de l'Ombre", "Rapière frappant depuis l'obscurité. Les ennemis tombent avant de voir Mist bouger.");
        AddFR(63, "Rapière Morsure de Givre", "Rapière d'acier gelé. Son toucher engourdit le corps et l'esprit.");
        AddFR(64, "Grâce de Camilla", "Un cadeau de quelqu'un que Mist chérissait. Elle combat pour honorer leur mémoire.");
        AddFR(65, "Rapière de Parade", "Rapière conçue pour la défense et l'attaque. Mist transforme chaque attaque en opportunité.");
        AddFR(66, "Rapière Radieuse", "Rapière émettant une lumière intérieure. Elle perce les mensonges et les ombres.");
        AddFR(67, "Rapière de l'Enfer", "Rapière brûlant d'un zèle inextinguible. La détermination de Mist ne peut être éteinte.");
        AddFR(68, "Rapière de Minuit", "Rapière forgée dans l'obscurité absolue. Elle frappe avec le silence de la mort.");
        AddFR(69, "Rapière d'Hiver", "Rapière sculptée dans la glace éternelle. Son froid ne peut être égalé que par la concentration de Mist au combat.");
        AddFR(70, "Rapière Indomptable", "La rapière légendaire du plus grand duelliste d'Astrid. Mist gagna ce titre par d'innombrables victoires.");
        
        // Nachia Weapons (71-88)
        AddFR(71, "Bâton de Jeune Pousse", "Jeune branche de la forêt des esprits. Même les jeunes pousses répondent à l'appel de Nachia.");
        AddFR(72, "Invocateur d'Esprits", "Bâton sculpté pour guider les voix des esprits de la forêt. Ils chuchotent des secrets à Nachia.");
        AddFR(73, "Branche de la Forêt", "Branche vivante qui pousse encore. La magie de la forêt coule à travers elle.");
        AddFR(74, "Bâton Sauvage", "Sauvage et imprévisible, comme Nachia elle-même. Les esprits aiment son énergie chaotique.");
        AddFR(75, "Bâton du Gardien", "Porté par ceux qui protègent la forêt. Nachia naquit avec cette responsabilité.");
        AddFR(76, "Baguette du Monde des Esprits", "Baguette reliant le monde matériel et le monde des esprits. Nachia marche entre les deux.");
        AddFR(77, "Bâton de Bois de Givre", "Bâton gelé dans l'hiver éternel. Les esprits froids du nord répondent à son appel.");
        AddFR(78, "Bâton de Racine d'Ombre", "Poussant dans les parties les plus profondes de la forêt. Les esprits d'ombre dansent autour.");
        AddFR(79, "Bâton de Bois de Braise", "Bâton qui ne cesse jamais de brûler. Les esprits de feu sont attirés par sa chaleur.");
        AddFR(80, "Bâton de Bénédiction Solaire", "Béni par les esprits de l'aube. Sa lumière guide les âmes perdues vers la maison.");
        AddFR(81, "Crocs de Fenrir", "Bâton avec des crocs de loup esprit au sommet. Le compagnon fidèle de Nachia guide son pouvoir.");
        AddFR(82, "Bâton de Vieux Chêne", "Sculpté dans un chêne millénaire. Les esprits les plus anciens se souviennent de sa plantation.");
        AddFR(83, "Branche de l'Arbre du Monde", "Branche de l'Arbre du Monde légendaire. Tous les esprits de la forêt s'inclinent devant lui.");
        AddFR(84, "Bâton du Roi des Esprits", "Le bâton du Roi des Esprits lui-même. Nachia le gagna en protégeant la forêt.");
        AddFR(85, "Bâton de Givre Éternel", "Bâton de glace éternelle du cœur gelé de la forêt. Les esprits d'hiver obéissent à ses ordres.");
        AddFR(86, "Perchoir du Phénix", "Bâton où les esprits de feu nichent. Ses flammes apportent la renaissance, pas la destruction.");
        AddFR(87, "Bâton du Bosquet Radieux", "Bâton émettant la lumière de mille lucioles. Les esprits d'espoir y dansent.");
        AddFR(88, "Bâton du Gardien du Vide", "Bâton gardant les frontières du monde. Les esprits d'ombre protègent son porteur.");
        
        // Shell Weapons (89-106)
        AddFR(89, "Katana du Novice", "Katana samouraï de base pour ceux qui apprennent la voie de l'épée. Shell le maîtrisa en quelques jours.");
        AddFR(90, "Katana Silencieux", "Katana samouraï conçu pour ne faire aucun bruit. Les victimes de Shell n'entendent jamais la mort venir.");
        AddFR(91, "Katana de l'Assassin", "Étroit et précis. Shell n'a besoin que d'un coup au bon endroit.");
        AddFR(92, "Katana Tueur", "Efficace. Pratique. Impitoyable. Comme Shell lui-même.");
        AddFR(93, "Katana du Chasseur", "Katana samouraï pour la chasse. Shell ne s'arrête pas jusqu'à ce que la cible soit éliminée.");
        AddFR(94, "Katana Vide", "Nommé d'après le vide que Shell ressent. Ou ne ressent pas.");
        AddFR(95, "Katana de l'Exécuteur", "Katana samouraï mettant fin à d'innombrables vies. Shell ne ressent rien pour aucune d'elles.");
        AddFR(96, "Katana de Précision", "Katana samouraï accordé pour exploiter les faiblesses. Shell calcule l'attaque optimale.");
        AddFR(97, "Katana Morsure de Givre", "Katana samouraï de malice froide. Son froid correspond au cœur vide de Shell.");
        AddFR(98, "Katana de Purification", "Katana samouraï lumineux pour un travail sombre. Shell ne ressent pas l'ironie.");
        AddFR(99, "Katana de l'Ombre", "Katana samouraï buvant les ombres. Shell se déplace invisiblement, frappe silencieusement.");
        AddFR(100, "Dague Ardente", "Katana samouraï chauffé par le feu intérieur. Les cibles de Shell brûlent avant de saigner.");
        AddFR(101, "Outil du Meurtrier Parfait", "L'outil ultime du meurtrier ultime. Shell est né pour cette épée.");
        AddFR(102, "Lame Impitoyable", "Katana samouraï qui ne s'émousse jamais, brandi par un poursuivant infatigable.");
        AddFR(103, "Assassin de l'Enfer", "Katana samouraï forgé dans le feu de l'enfer. Il brûle avec l'intensité du seul but de Shell.");
        AddFR(104, "Katana du Vide", "Katana samouraï d'obscurité absolue. Comme Shell, il n'existe que pour terminer.");
        AddFR(105, "Katana Gelé", "Katana samouraï de glace éternelle. Son jugement est aussi froid et final que Shell.");
        AddFR(106, "Fin Radieuse", "La forme ultime du katana samouraï lumineux. Shell exécute des jugements au nom d'El.");
        
        // Vesper Weapons (107-124)
        AddFR(107, "Sceptre du Novice", "Sceptre simple pour les nouveaux membres de l'Ordre de la Flamme Solaire. Vesper commença son voyage ici.");
        AddFR(108, "Marteau du Jugement", "Marteau de guerre utilisé pour exécuter la volonté d'El. Chaque coup est un jugement sacré.");
        AddFR(109, "Bâton du Gardien du Feu", "Arme portée par ceux qui gardent le feu sacré. Ses coups purifient l'impureté.");
        AddFR(110, "Bouclier de l'Acolyte", "Bouclier simple pour les acolytes de l'Ordre. La foi est la meilleure défense.");
        AddFR(111, "Bouclier de l'Inquisiteur", "Bouclier porté par les inquisiteurs. Il a vu la chute d'innombrables hérétiques.");
        AddFR(112, "Défenseur du Crépuscule", "Bouclier gardant entre la lumière et l'obscurité.");
        AddFR(113, "Marteau du Fanatique", "Marteau de guerre brûlant de fanatisme religieux. Les ennemis tremblent devant lui.");
        AddFR(114, "Marteau Lourd de Purification", "Énorme marteau de guerre utilisé pour purifier le mal. Un coup écrase le péché.");
        AddFR(115, "Jugement d'El", "Marteau de guerre béni personnellement par El. Son jugement est sans appel.");
        AddFR(116, "Bastion de la Foi", "Bouclier indestructible. Tant que la foi ne s'éteint pas, il ne se brisera pas.");
        AddFR(117, "Bouclier Sacré de Flamme", "Bouclier entouré de feu sacré. Le mal qui le touche est brûlé.");
        AddFR(118, "Bastion de l'Inquisition", "Bouclier d'un haut inquisiteur. Vesper le gagna par un service impitoyable.");
        AddFR(119, "Briseur de l'Aube", "Marteau de guerre légendaire écrasant l'obscurité. Vesper brandit l'incarnation de la colère d'El.");
        AddFR(120, "Destructeur Sacré", "Marteau de guerre béni par le grand prêtre. Ses coups portent le poids de la colère sacrée.");
        AddFR(121, "Main Droite d'El", "L'arme la plus sacrée de l'Ordre. Vesper est l'outil choisi de la volonté d'El.");
        AddFR(122, "Forteresse de Flamme", "La défense ultime de l'Ordre. La lumière d'El protège Vesper de tout mal.");
        AddFR(123, "Vigilance Éternelle", "Bouclier inébranlable. Comme Vesper, il veille toujours sur l'obscurité.");
        AddFR(124, "Serment du Paladin du Crépuscule", "Bouclier du chef de l'Ordre. Vesper jura de lier son âme à El.");
        
        // Yubar Weapons (125-142)
        AddFR(125, "Étoile Nouvelle-Née", "Une étoile qui vient de se former. Yubar se souvient quand toutes les étoiles étaient si petites.");
        AddFR(126, "Braise Cosmique", "Fragment de feu stellaire. Il brûle avec la chaleur de la création elle-même.");
        AddFR(127, "Sphère de Poussière d'Étoiles", "Poussière d'étoiles compressée attendant d'être façonnée. Yubar tisse des galaxies avec une telle matière.");
        AddFR(128, "Catalyseur de Rêve", "Sphère amplifiant l'énergie des rêves. Provenant des premières expériences de Yubar.");
        AddFR(129, "Cœur de Nébuleuse", "Le cœur d'une nébuleuse lointaine. Yubar le cueillit de la tapisserie cosmique.");
        AddFR(130, "Graine Céleste", "Graine qui deviendra une étoile. Yubar cultive d'innombrables telles graines.");
        AddFR(131, "Fragment de Supernova", "Fragment d'une étoile explosée. Yubar fut témoin de sa mort et préserva son pouvoir.");
        AddFR(132, "Puits Gravitationnel", "Sphère compressant la gravité. L'espace lui-même se courbe autour.");
        AddFR(133, "Singularité du Vide", "Point de densité infinie. Yubar l'utilise pour créer et détruire des mondes.");
        AddFR(134, "Couronne Solaire", "Fragment de l'atmosphère extérieure du soleil. Il brûle avec la colère de l'étoile.");
        AddFR(135, "Comète Gelée", "Noyau de comète capturé. Il porte des secrets des profondeurs du cosmos.");
        AddFR(136, "Forge Stellaire", "Miniature d'un noyau stellaire. Yubar l'utilise pour forger une nouvelle réalité.");
        AddFR(137, "Relique du Big Bang", "Fragment du Big Bang de la création. Il contient l'origine de l'univers.");
        AddFR(138, "Métier Cosmique", "Outil tissant la réalité. Yubar l'utilise pour remodeler le destin.");
        AddFR(139, "Création Radieuse", "La lumière de la création elle-même. Yubar l'utilisa pour donner naissance aux premières étoiles.");
        AddFR(140, "Cœur de l'Entropie", "L'essence de la décomposition cosmique. Tout se termine, et Yubar décide quand.");
        AddFR(141, "Zéro Absolu", "Le point le plus froid de l'existence. Même le temps gèle devant lui.");
        AddFR(142, "Cœur de Catastrophe", "Le cœur de la destruction cosmique. Yubar ne le libère qu'en moments critiques.");
        
        // Rings (143-162)
        AddFR(143, "Anneau du Rêveur", "Anneau simple porté par ceux qui entrent juste dans le monde des rêves. Il émet un faible espoir.");
        AddFR(144, "Anneau de Poussière d'Étoiles", "Forgé de poussière d'étoiles condensée. Les rêveurs disent qu'il brille plus fort en danger.");
        AddFR(145, "Marque du Sommeil", "Gravure d'un sommeil paisible. Les porteurs récupèrent plus vite de leurs blessures.");
        AddFR(146, "Anneau d'Esprit", "Anneau capturant les esprits errants. Leur lumière guide les chemins à travers les rêves sombres.");
        AddFR(147, "Anneau du Vagabond", "Porté par les vagabonds des royaumes des rêves. Il a vu d'innombrables chemins oubliés.");
        AddFR(148, "Anneau de Fragment de Cauchemar", "Anneau contenant des fragments de cauchemars vaincus. Son obscurité renforce les braves.");
        AddFR(149, "Gardien de la Mémoire", "Anneau préservant de précieux souvenirs. Smoothie vend des similaires dans sa boutique.");
        AddFR(150, "Anneau de l'Arcanum Lunaire", "Anneau de la Société Arcanum Lunaire. Aurena en portait un similaire avant son exil.");
        AddFR(151, "Marque de la Forêt des Esprits", "Anneau béni par les esprits de la forêt de Nachia. La force de la nature coule à travers.");
        AddFR(152, "Insigne de la Garde Royale", "Anneau porté par Lacerta pendant son service. Il porte toujours le poids de la responsabilité.");
        AddFR(153, "Gloire du Duelliste", "Anneau transmis parmi les duellistes nobles. La famille de Mist chérit de tels anneaux.");
        AddFR(154, "Amulette de Braise de Flamme", "Étincelle scellée du feu sacré. L'Ordre de Vesper garde ces reliques.");
        AddFR(155, "Fragment de Livre de Sorts", "Anneau fait de magie cristalline de Bismuth. Il bourdonne d'énergie arcanum.");
        AddFR(156, "Marque du Chasseur", "Anneau traquant la proie. Les contrôleurs de Shell les utilisent pour surveiller leurs outils.");
        AddFR(157, "Bénédiction d'El", "Anneau béni par El lui-même. Sa lumière dissipe les cauchemars et guérit les blessés.");
        AddFR(158, "Marque du Seigneur des Cauchemars", "Pris d'un seigneur de cauchemar vaincu. Son pouvoir sombre corrompt et renforce.");
        AddFR(159, "Héritage d'Astrid", "Anneau de la maison noble d'Astrid. Les ancêtres de Mist le portèrent dans d'innombrables duels.");
        AddFR(160, "Œil du Voyant", "Anneau capable de voir à travers le voile des rêves. Passé, présent et futur se mélangent.");
        AddFR(161, "Anneau du Rêve Primitif", "Forgé à l'aube des rêves. Il contient l'essence du premier rêveur.");
        AddFR(162, "Sommeil Éternel", "Anneau touchant le sommeil le plus profond. Les porteurs ne craignent ni la mort ni le réveil.");
        
        // Amulets (163-183)
        AddFR(163, "Pendentif du Rêveur", "Pendentif simple porté par ceux qui entrent juste dans le monde des rêves. Il émet un faible espoir.");
        AddFR(164, "Pendentif du Toucher de Feu", "Pendentif touché par le feu. Sa chaleur dissipe le froid et la peur.");
        AddFR(165, "Larme Écarlate", "Rubis en forme de larme de sang. Son pouvoir vient du sacrifice.");
        AddFR(166, "Amulette d'Ambre", "Ambre contenant une créature ancienne. Ses souvenirs renforcent le porteur.");
        AddFR(167, "Médaille du Crépuscule", "Médaille scintillant entre la lumière et l'obscurité. Les nouveaux membres de Vesper portent celles-ci.");
        AddFR(168, "Collier de Poussière d'Étoiles", "Collier saupoudré d'étoiles cristallines. Smoothie les collecte des rêves tombés.");
        AddFR(169, "Œil d'Émeraude", "Gemme verte capable de voir à travers les illusions. Les cauchemars ne peuvent échapper à son regard.");
        AddFR(170, "Cœur de l'Esprit de la Forêt", "Gemme contenant la bénédiction des esprits de la forêt. Les gardiens de Nachia chérissent celles-ci.");
        AddFR(171, "Marque de l'Arcanum Lunaire", "Marque des archives interdites. Aurena risque tout en les étudiant.");
        AddFR(172, "Cristal de Bismuth", "Fragment de magie pure cristalline. Il résonne avec l'énergie arcanum.");
        AddFR(173, "Amulette de Braise de Flamme", "Étincelle scellée du feu sacré. L'Ordre de Vesper garde ces reliques.");
        AddFR(174, "Pendentif de Croc de Cauchemar", "Croc d'un cauchemar puissant, maintenant un trophée. Shell en porte un en souvenir.");
        AddFR(175, "Médaille de la Garde Royale", "Médaille d'honneur de la Garde Royale. Lacerta en gagna beaucoup avant son exil.");
        AddFR(176, "Blason d'Astrid", "Blason noble de la famille d'Astrid. L'héritage de la famille de Mist pend à cette chaîne.");
        AddFR(177, "Larme Sacrée d'El", "Larme versée par El lui-même. Sa lumière protège contre les cauchemars les plus sombres.");
        AddFR(178, "Émeraude Primitive", "Gemme du premier forêt. Elle contient les rêves d'esprits anciens.");
        AddFR(179, "Œil du Seigneur des Cauchemars", "Œil d'un seigneur de cauchemar vaincu. Il voit toutes les peurs et les exploite.");
        AddFR(180, "Cœur de la Flamme", "Le cœur du feu sacré lui-même. Seuls les plus pieux peuvent le porter.");
        AddFR(181, "Oracle du Voyant", "Amulette capable de voir tous les futurs possibles. La réalité se courbe devant ses prophéties.");
        AddFR(182, "Boussole du Voyageur du Vide", "Amulette pointant vers le vide. Ceux qui le suivent ne reviennent jamais de la même manière.");
        AddFR(183, "Amulette de Midas", "Amulette légendaire touchée par le rêve de l'or. Les ennemis vaincus laissent tomber de l'or supplémentaire.");
        AddFR(478, "Collecteur de Poussière d'Étoiles", "Amulette légendaire qui collecte la poussière d'étoiles cristallisée des ennemis vaincus. Chaque élimination produit une précieuse essence de rêve (base 1-5, s'adapte au niveau d'amélioration et à la force du monstre).");
        
        // Belts (184-203)
        AddFR(184, "Ceinture du Vagabond", "Ceinture de tissu simple portée par ceux qui errent entre les rêves. Elle ancre l'âme.");
        AddFR(185, "Corde du Rêveur", "Corde tissée de fils de rêve. Elle pulse doucement à chaque battement de cœur.");
        AddFR(186, "Ceinture de Poussière d'Étoiles", "Ceinture ornée de poussière d'étoiles cristalline. Smoothie vend des accessoires similaires.");
        AddFR(187, "Ceinture de la Sentinelle", "Ceinture solide portée par les gardiens des rêves. Elle ne se desserre jamais, ne faillit jamais.");
        AddFR(188, "Ceinture Mystique", "Ceinture imprégnée de magie faible. Elle pique quand les cauchemars approchent.");
        AddFR(189, "Ceinture du Chasseur", "Ceinture de cuir avec des sacs de provisions. Essentielle pour les longs voyages dans les rêves.");
        AddFR(190, "Ceinture du Pèlerin", "Portée par ceux qui cherchent la lumière d'El. Elle offre du réconfort dans les rêves les plus sombres.");
        AddFR(191, "Ceinture du Chasseur de Cauchemars", "Ceinture faite de cauchemars vaincus. Leur essence renforce le porteur.");
        AddFR(192, "Ceinture Arcanum", "Ceinture de la Société Arcanum Lunaire. Elle amplifie l'énergie magique.");
        AddFR(193, "Ceinture de Flamme", "Ceinture bénie par l'Ordre de Vesper. Elle brûle avec le feu de la protection.");
        AddFR(194, "Ceinture d'Épée du Duelliste", "Ceinture élégante portée par les guerriers nobles d'Astrid. Elle porte des épées et l'honneur.");
        AddFR(195, "Corde du Tisseur d'Esprits", "Ceinture tissée par les esprits de la forêt pour Nachia. Elle bourdonne d'énergie naturelle.");
        AddFR(196, "Ceinture d'Outils de l'Assassin", "Ceinture avec des compartiments cachés. Les contrôleurs de Shell les utilisent pour équiper leurs outils.");
        AddFR(197, "Ceinture d'Étui de Revolver", "Ceinture conçue pour les armes à feu de Lacerta. Tir rapide, mise à mort plus rapide.");
        AddFR(198, "Ceinture Sacrée d'El", "Ceinture bénie par El lui-même. Sa lumière ancre l'âme contre toute obscurité.");
        AddFR(199, "Chaîne du Seigneur des Cauchemars", "Ceinture forgée des chaînes d'un seigneur de cauchemar. Elle lie la peur à la volonté du porteur.");
        AddFR(200, "Ceinture du Rêve Primitif", "Ceinture du premier rêve. Elle contient l'écho de la création elle-même.");
        AddFR(201, "Ceinture de Racine de l'Arbre du Monde", "Ceinture poussant des racines de l'Arbre du Monde. La forêt de Nachia bénit sa création.");
        AddFR(202, "Héritage d'Astrid", "Ceinture de famille de la famille d'Astrid. Le sang de Mist est tissé dans son tissu.");
        AddFR(203, "Ceinture de l'Inquisiteur du Crépuscule", "Ceinture du plus haut niveau de Vesper. Elle juge tous ceux qui se tiennent devant elle.");
        
        // Armor Sets - Bone Sentinel (204-215)
        AddFR(204, "Masque de la Sentinelle d'Os", "Masque sculpté du crâne d'un cauchemar tombé. Ses yeux vides voient à travers les illusions.");
        AddFR(205, "Casque du Gardien du Monde des Esprits", "Casque imprégné des esprits de la forêt de Nachia. Des chuchotements de protection entourent le porteur.");
        AddFR(206, "Couronne du Seigneur des Cauchemars", "Couronne forgée de cauchemars conquis. Le porteur règne sur la peur elle-même.");
        AddFR(207, "Gilet de la Sentinelle d'Os", "Armure des côtes de créatures mortes dans les rêves. Leur protection dure au-delà de la mort.");
        AddFR(208, "Cuirasse du Gardien du Monde des Esprits", "Cuirasse tissée de rêves cristallisés. Elle bouge et s'adapte aux coups entrants.");
        AddFR(209, "Cuirasse du Seigneur des Cauchemars", "Armure pulsant d'énergie sombre. Les cauchemars s'inclinent devant son porteur.");
        AddFR(210, "Jambières de la Sentinelle d'Os", "Jambières renforcées d'os de cauchemar. Elles ne se fatiguent jamais, ne cèdent jamais.");
        AddFR(211, "Jambières du Gardien du Monde des Esprits", "Jambières bénies par les esprits de la forêt. Le porteur se déplace comme le vent à travers les feuilles.");
        AddFR(212, "Jambières du Seigneur des Cauchemars", "Jambières buvant les ombres. L'obscurité renforce chaque pas.");
        AddFR(213, "Bottes de la Sentinelle d'Os", "Bottes pour marcher silencieusement dans les royaumes des rêves. Les morts ne font pas de bruit.");
        AddFR(214, "Bottes de Fer du Gardien du Monde des Esprits", "Bottes qui ne laissent aucune trace dans les royaumes des rêves. Parfaites pour les chasseurs de cauchemars.");
        AddFR(215, "Bottes du Seigneur des Cauchemars", "Bottes traversant les mondes. La réalité se courbe sous chaque pas.");
        
        // Aurena Armor (216-243)
        AddFR(216, "Chapeau d'Apprenti d'Aurena", "Chapeau simple porté par les novices de l'Arcanum Lunaire.");
        AddFR(217, "Turban Mystique d'Aurena", "Turban imprégné d'énergie lunaire faible.");
        AddFR(218, "Couronne Ensorcelante d'Aurena", "Couronne renforçant l'affinité magique du porteur.");
        AddFR(219, "Couronne Ancienne d'Aurena", "Relique ancienne transmise à travers les générations.");
        AddFR(220, "Bandeau de Faucon d'Aurena", "Bandeau béni par les esprits du ciel.");
        AddFR(221, "Couronne de Pouvoir d'Aurena", "Couronne puissante émettant le pouvoir arcanum.");
        AddFR(222, "Couronne Lunaire d'Aurena", "Couronne légendaire d'un maître de l'Arcanum Lunaire.");
        AddFR(223, "Robe Bleue d'Aurena", "Robe simple portée par les magiciens lunaires.");
        AddFR(224, "Robe de Magicien d'Aurena", "Robe chérie par les magiciens pratiquants.");
        AddFR(225, "Robe Tissée de Sorts d'Aurena", "Robe tissée de fils magiques.");
        AddFR(226, "Robe Illuminante d'Aurena", "Robe guidant la sagesse lunaire.");
        AddFR(227, "Linceul de Rêve d'Aurena", "Linceul tissé de rêves cristallisés.");
        AddFR(228, "Robe Royale à Écailles d'Aurena", "Robe convenant à la famille royale lunaire.");
        AddFR(229, "Robe de la Reine des Glaces d'Aurena", "Robe légendaire de la Reine des Glaces elle-même.");
        AddFR(230, "Jambières Bleues d'Aurena", "Jambières simples portées par les magiciens aspirants.");
        AddFR(231, "Jupe de Fibre d'Aurena", "Jupe légère permettant un mouvement libre.");
        AddFR(232, "Jambières Elfiques d'Aurena", "Jambières élégantes conçues par les elfes.");
        AddFR(233, "Jambières Illuminantes d'Aurena", "Jambières bénies par la sagesse.");
        AddFR(234, "Jupe Glaciaire d'Aurena", "Jupe imprégnée de magie de givre.");
        AddFR(235, "Pantalon de Givre d'Aurena", "Pantalon émettant la force du froid.");
        AddFR(236, "Jambières Magnifiques d'Aurena", "Jambières légendaires d'élégance incomparable.");
        AddFR(237, "Pantoufles Douces de Lune", "Chaussures douces portées par les acolytes de l'Arcanum Lunaire. Elles amortissent chaque pas avec la lumière lunaire.");
        AddFR(238, "Sandales du Pèlerin", "Sandales simples bénies par les anciens de la Société. Aurena se souvient encore de leur chaleur.");
        AddFR(239, "Chaussures de Mystique Orientale", "Chaussures exotiques de terres lointaines. Elles guident l'énergie de guérison à travers la terre.");
        AddFR(240, "Grâce du Marcheur de Rêve", "Bottes marchant entre le monde éveillé et le monde des rêves. Parfaites pour les guérisseurs cherchant des âmes perdues.");
        AddFR(241, "Bottes du Marcheur d'Âmes", "Bottes imprégnées de l'essence de guérisseurs décédés. Leur sagesse guide chaque pas.");
        AddFR(242, "Bottes du Traqueur d'Âmes", "Bottes sombres traquant les blessés. Aurena les utilise pour trouver ceux qui ont besoin d'aide... ou ceux qui l'ont blessée.");
        AddFR(243, "Ailes du Sage Déchu", "Bottes légendaires censées donner la capacité de voler aux cœurs purs. L'exil d'Aurena prouva sa qualification.");
        
        // Bismuth Armor (244-271)
        AddFR(244, "Chapeau Conique de Bismuth", "Chapeau simple pour les apprentis cristallins.");
        AddFR(245, "Chapeau d'Émeraude de Bismuth", "Chapeau incrusté de cristaux d'émeraude.");
        AddFR(246, "Masque Glaciaire de Bismuth", "Masque fait de cristaux gelés.");
        AddFR(247, "Masque Alarharim de Bismuth", "Ancien masque de pouvoir cristallin.");
        AddFR(248, "Casque Prismatique de Bismuth", "Casque réfractant la lumière en arc-en-ciel.");
        AddFR(249, "Couronne Réfléchissante de Bismuth", "Couronne réfléchissant l'énergie magique.");
        AddFR(250, "Couronne Incandescente de Bismuth", "Couronne légendaire brûlant avec le feu cristallin.");
        AddFR(251, "Robe de Glace de Bismuth", "Robe refroidie par la magie cristalline.");
        AddFR(252, "Robe de Moine de Bismuth", "Robe simple pour la méditation.");
        AddFR(253, "Robe Glaciaire de Bismuth", "Robe gelée dans le givre éternel.");
        AddFR(254, "Armure Cristalline de Bismuth", "Armure formée de cristaux vivants.");
        AddFR(255, "Armure Prismatique de Bismuth", "Armure courbant la lumière elle-même.");
        AddFR(256, "Armure de Plaques Gelée de Bismuth", "Armure de plaques de givre éternel.");
        AddFR(257, "Armure de Plaques Sacrée de Bismuth", "Armure légendaire bénie par les dieux cristallins.");
        AddFR(258, "Jambières de Cotte de Mailles de Bismuth", "Jambières de base avec des maillons cristallins.");
        AddFR(259, "Jambières de Fibre de Bismuth", "Jambières légères pour les magiciens cristallins.");
        AddFR(260, "Jambières d'Émeraude de Bismuth", "Jambières ornées de cristaux d'émeraude.");
        AddFR(261, "Culotte Étrange de Bismuth", "Culotte pulsant d'énergie étrange.");
        AddFR(262, "Jambières Prismatiques de Bismuth", "Jambières scintillant de lumière prismatique.");
        AddFR(263, "Jambières Alarharim de Bismuth", "Anciennes jambières d'Alarharim.");
        AddFR(264, "Jambières de Faucon de Bismuth", "Jambières légendaires de l'Ordre du Faucon.");
        AddFR(265, "Pantoufles du Savant", "Chaussures confortables pour un long travail dans les bibliothèques. La connaissance coule à travers leurs semelles.");
        AddFR(266, "Enveloppements Alarharim", "Anciens enveloppements d'une civilisation perdue. Ils chuchotent des sorts oubliés.");
        AddFR(267, "Chaussures de Glace", "Bottes sculptées dans la glace éternelle. Bismuth laisse des motifs de givre où elle marche.");
        AddFR(268, "Pas de Fleur de Givre", "Bottes élégantes fleurissant de cristaux de glace. Chaque pas crée un jardin de givre.");
        AddFR(269, "Bottes de Résonance Cristalline", "Bottes amplifiant l'énergie magique. Les cristaux bourdonnent de pouvoir inexploité.");
        AddFR(270, "Bottes Arcanum Prismatiques", "Bottes réfractant la lumière en énergie magique pure. La réalité se courbe autour de chaque pas.");
        AddFR(271, "Bottes du Voyageur du Vide", "Bottes légendaires traversant le vide lui-même. Bismuth voit des chemins que d'autres ne peuvent imaginer.");
        
        // Lacerta Armor (272-299)
        AddFR(272, "Casque de Cuir d'Ezéchias", "Casque de base pour les guerriers dragons.");
        AddFR(273, "Casque de Cotte de Mailles d'Ezéchias", "Casque de cotte de mailles solide.");
        AddFR(274, "Casque de Guerrier d'Ezéchias", "Casque porté par des guerriers expérimentés.");
        AddFR(275, "Casque d'Écailles de Dragon d'Ezéchias", "Casque forgé d'écailles de dragon.");
        AddFR(276, "Casque Royal d'Ezéchias", "Casque convenant à la famille royale dragon.");
        AddFR(277, "Casque d'Élite Homme-Dragon d'Ezéchias", "Casque d'élite de l'Ordre Homme-Dragon.");
        AddFR(278, "Casque d'Or d'Ezéchias", "Casque d'or légendaire du Roi Dragon.");
        AddFR(279, "Armure de Cuir d'Ezéchias", "Armure de base pour les guerriers dragons.");
        AddFR(280, "Armure d'Écailles d'Ezéchias", "Armure renforcée d'écailles.");
        AddFR(281, "Armure de Chevalier d'Ezéchias", "Armure lourde pour les chevaliers dragons.");
        AddFR(282, "Cotte de Mailles d'Écailles de Dragon d'Ezéchias", "Cotte de mailles forgée d'écailles de dragon.");
        AddFR(283, "Cotte de Mailles Royale Homme-Dragon d'Ezéchias", "Cotte de mailles royale de l'Ordre Homme-Dragon.");
        AddFR(284, "Armure de Plaques de Faucon d'Ezéchias", "Armure de plaques de l'Ordre du Faucon.");
        AddFR(285, "Armure d'Or d'Ezéchias", "Armure d'or légendaire du Roi Dragon.");
        AddFR(286, "Jambières de Cuir d'Ezéchias", "Jambières de base pour les guerriers dragons.");
        AddFR(287, "Jambières Rivetées d'Ezéchias", "Jambières renforcées de rivets.");
        AddFR(288, "Jambières de Chevalier d'Ezéchias", "Jambières lourdes pour les chevaliers dragons.");
        AddFR(289, "Jambières d'Écailles de Dragon d'Ezéchias", "Jambières forgées d'écailles de dragon.");
        AddFR(290, "Jambières Royales d'Ezéchias", "Jambières convenant à la famille royale.");
        AddFR(291, "Jambières Magnifiques d'Ezéchias", "Jambières magnifiques d'artisanat de maître.");
        AddFR(292, "Jambières d'Or d'Ezéchias", "Jambières d'or légendaires du Roi Dragon.");
        AddFR(293, "Bottes de Cuir Usées", "Bottes simples ayant vu de nombreuses batailles. Elles portent le parfum de l'aventure.");
        AddFR(294, "Bottes de Combat Rapiécées", "Bottes réparées d'innombrables fois. Chaque pièce raconte une histoire de survie.");
        AddFR(295, "Bottes du Guerrier d'Acier", "Bottes lourdes forgées pour le combat. Elles ancrent Lacerta au sol pendant la bataille.");
        AddFR(296, "Jambières du Gardien", "Bottes portées par les gardiens d'élite. Elles ne reculent jamais, ne se rendent jamais.");
        AddFR(297, "Jambières d'Écailles de Dragon", "Bottes faites d'écailles de dragon. Elles confèrent au porteur l'endurance du dragon.");
        AddFR(298, "Bottes du Seigneur de Guerre Homme-Dragon", "Bottes d'anciens généraux homme-dragon. Redoutables sur n'importe quel champ de bataille.");
        AddFR(299, "Bottes du Maître d'Or", "Bottes légendaires du plus grand guerrier d'une ère. Le destin de Lacerta attend.");
        
        // Mist Armor (300-327)
        AddFR(300, "Turban de Mist", "Turban simple cachant l'identité.");
        AddFR(301, "Turban Léger de Mist", "Turban léger et respirant.");
        AddFR(302, "Turban de Vision Nocturne de Mist", "Turban renforçant la vision nocturne.");
        AddFR(303, "Bandeau d'Éclair de Mist", "Bandeau chargé d'énergie électrique.");
        AddFR(304, "Capuche de Cobra de Mist", "Capuche mortelle comme un serpent.");
        AddFR(305, "Masque de Traqueur d'Enfer de Mist", "Masque traquant la proie.");
        AddFR(306, "Chuchoteur Sombre de Mist", "Masque légendaire chuchotant la mort.");
        AddFR(307, "Harnais de Cuir de Mist", "Harnais léger pour l'agilité.");
        AddFR(308, "Cape de Forestier de Mist", "Cape pour mouvement rapide.");
        AddFR(309, "Tunique de Minuit de Mist", "Tunique pour frappes nocturnes.");
        AddFR(310, "Robe d'Éclair de Mist", "Robe chargée d'éclairs.");
        AddFR(311, "Armure de Tension de Mist", "Armure pulsant d'énergie électrique.");
        AddFR(312, "Armure de Maître Archer de Mist", "Armure d'un maître archer.");
        AddFR(313, "Cape de Seigneur Sombre de Mist", "Cape légendaire du Seigneur Sombre.");
        AddFR(314, "Jambières de Forestier de Mist", "Jambières portées par les forestiers.");
        AddFR(315, "Jambières de Survie de Jungle de Mist", "Jambières de survie de jungle.");
        AddFR(316, "Sarong de Minuit de Mist", "Sarong pour opérations nocturnes.");
        AddFR(317, "Jambières d'Éclair de Mist", "Jambières chargées d'éclairs.");
        AddFR(318, "Jambières de Sauterelle de Mist", "Jambières pour sauts étonnants.");
        AddFR(319, "Jambières de Démon Vert de Mist", "Jambières de vitesse démoniaque.");
        AddFR(320, "Jambières du Marcheur d'Âmes de Mist", "Jambières légendaires traversant les mondes.");
        AddFR(321, "Bottes d'Éclaireur Temporaires", "Bottes assemblées de fragments. Légères et silencieuses, parfaites pour l'éclaireur.");
        AddFR(322, "Bottes du Coureur de Marais", "Bottes étanches pour terrain difficile. Mist connaît chaque raccourci.");
        AddFR(323, "Marcheur Rapide", "Bottes enchantées accélérant les pas du porteur. Parfaites pour la tactique frapper-et-courir.");
        AddFR(324, "Bottes du Chasseur de Cerf", "Bottes traquant la proie.");
        AddFR(325, "Bottes de Coup d'Éclair", "Bottes chargées d'énergie d'éclair.");
        AddFR(326, "Bottes de Combat du Marcheur de Feu", "Bottes capables de marcher à travers le feu.");
        AddFR(327, "Piétineur de Souffrance", "Bottes légendaires piétinant toute souffrance.");
        
        // Nachia Armor (328-355)
        AddFR(328, "Chapeau de Fourrure de Nachia", "Chapeau simple pour chasseurs de forêt.");
        AddFR(329, "Coiffe à Plumes de Nachia", "Coiffe ornée de plumes.");
        AddFR(330, "Masque de Chamane de Nachia", "Masque de pouvoir spirituel.");
        AddFR(331, "Capuche de Terre de Nachia", "Capuche bénie par les esprits de la terre.");
        AddFR(332, "Casque de Nature de Nachia", "Casque imprégné de force naturelle.");
        AddFR(333, "Couronne d'Arbre de Nachia", "Couronne poussant d'un vieil arbre.");
        AddFR(334, "Couronne de Feuille de Nachia", "Couronne légendaire du Gardien de la Forêt.");
        AddFR(335, "Armure de Fourrure de Nachia", "Armure faite de créatures de forêt.");
        AddFR(336, "Armure Indigène de Nachia", "Armure traditionnelle des tribus de forêt.");
        AddFR(337, "Manteau de Bois Vert de Nachia", "Manteau tissé de vignes vivantes.");
        AddFR(338, "Cape de Terre de Nachia", "Cape bénie par les esprits de la terre.");
        AddFR(339, "Étreinte de Nature de Nachia", "Armure unie avec la nature.");
        AddFR(340, "Armure de Nid de Marais de Nachia", "Armure des marais profonds.");
        AddFR(341, "Robe de Feuille de Nachia", "Robe légendaire du Gardien de la Forêt.");
        AddFR(342, "Shorts de Fourrure de Mammouth de Nachia", "Shorts chauds faits de fourrure de mammouth.");
        AddFR(343, "Shorts de Cuir de Nachia", "Jambières de chasse traditionnelles.");
        AddFR(344, "Jambières de Cerf de Nachia", "Jambières faites de peau de cerf.");
        AddFR(345, "Jambières de Terre de Nachia", "Jambières bénies par les esprits de la terre.");
        AddFR(346, "Jambières de Feuille de Nachia", "Jambières tissées de feuilles magiques.");
        AddFR(347, "Pagne d'Homme-Sanglier de Nachia", "Pagne d'un puissant homme-sanglier.");
        AddFR(348, "Jambières de Chasse Sanglante", "Jambières légendaires de chasse sanglante.");
        AddFR(349, "Bottes Doublées de Fourrure", "Bottes chaudes et confortables.");
        AddFR(350, "Bottes de Peau de Blaireau", "Bottes faites de peau de blaireau.");
        AddFR(351, "Bottes de Coup de Cobra", "Bottes rapides comme un serpent.");
        AddFR(352, "Enveloppements du Rôdeur de Forêt", "Enveloppements silencieux dans la forêt.");
        AddFR(353, "Bottes du Chasseur de Terre", "Bottes traquant la proie.");
        AddFR(354, "Bottes de Forestier de Fleur de Fièvre", "Bottes ornées de fleurs de fièvre. Elles confèrent une vitesse fiévreuse et une précision mortelle.");
        AddFR(355, "Bottes du Prédateur Sanglant", "Bottes légendaires trempées du sang d'innombrables chasses. Nachia devint le prédateur suprême.");
        
        // Shell Armor (356-383)
        AddFR(356, "Casque Endommagé de Shell", "Casque endommagé du monde souterrain.");
        AddFR(357, "Masque Cassé de Shell", "Masque cassé offrant toujours protection.");
        AddFR(358, "Casque de Seigneur d'Os de Shell", "Casque forgé des restes du Seigneur d'Os.");
        AddFR(359, "Casque de Squelette de Shell", "Casque formé d'os.");
        AddFR(360, "Casque de Mort de Shell", "Casque de la mort elle-même.");
        AddFR(361, "Garde de Squelette Nokferatu de Shell", "Casque d'os de vampire.");
        AddFR(362, "Casque Démoniaque de Shell", "Casque légendaire du Seigneur Démon.");
        AddFR(363, "Linceul Funéraire de Shell", "Linceul de la tombe.");
        AddFR(364, "Vieille Cape de Shell", "Vieille cape des temps anciens.");
        AddFR(365, "Cape d'Os Nokferatu de Shell", "Cape tissée d'os.");
        AddFR(366, "Cape d'Âmes de Shell", "Cape d'âmes agitées.");
        AddFR(367, "Robe de Mort de Shell", "Robe de la mort.");
        AddFR(368, "Robe du Monde Souterrain de Shell", "Robe des profondeurs.");
        AddFR(369, "Armure Démoniaque de Shell", "Armure légendaire du Seigneur Démon.");
        AddFR(370, "Tablier Endommagé de Shell", "Tablier endommagé mais utile.");
        AddFR(371, "Tablier d'Os Muté de Shell", "Tablier fait d'os mutés.");
        AddFR(372, "Enveloppements d'Épines Nokferatu de Shell", "Enveloppements avec épines.");
        AddFR(373, "Garde de Chair Nokferatu de Shell", "Protection faite de chair conservée.");
        AddFR(374, "Jambières Sanguines de Shell", "Jambières trempées de sang.");
        AddFR(375, "Marcheur de Sang Nokferatu de Shell", "Jambières marchant dans le sang.");
        AddFR(376, "Jambières Démoniaques de Shell", "Jambières légendaires du Seigneur Démon.");
        AddFR(377, "Jambières Blindées", "Bottes métalliques lourdes écrasant tout sous les pieds. La défense est la meilleure attaque.");
        AddFR(378, "Bottes du Navigateur", "Bottes solides résistant à toute tempête. Elles ancrent Shell dans les courants de bataille.");
        AddFR(379, "Jambières des Profondeurs", "Bottes forgées dans les profondeurs de l'océan. Résistantes à toute pression.");
        AddFR(380, "Bottes Écrasant les Os", "Bottes renforcées d'os de monstres. Chaque pas est une menace.");
        AddFR(381, "Bottes de Forteresse de Magma", "Bottes forgées dans le feu du volcan. Elles rayonnent de chaleur brûlant les attaquants.");
        AddFR(382, "Bottes du Piétineur de Sang", "Bottes brutales laissant des traces de destruction. Les ennemis craignent de les combattre.");
        AddFR(383, "Bottes du Gardien Inébranlable", "Bottes légendaires d'un défenseur indestructible. Shell devient une forteresse immobile.");
        
        // Vesper Armor (384-411)
        AddFR(384, "Chapeau de Sorcière de Vesper", "Chapeau simple pour tisseurs de rêves.");
        AddFR(385, "Capuche Étrange de Vesper", "Capuche chuchotant des secrets.");
        AddFR(386, "Capuche Étrange de Vesper", "Capuche pulsant d'énergie étrange.");
        AddFR(387, "Capuche d'Énergie de Cauchemar de Vesper", "Capuche d'énergie de cauchemar.");
        AddFR(388, "Enveloppements de Désespoir de Vesper", "Enveloppements se nourrissant de désespoir.");
        AddFR(389, "Couronne de Sorcier Sombre de Vesper", "Couronne de magie sombre.");
        AddFR(390, "Chapeau de Ferunbras de Vesper", "Chapeau légendaire du Maître des Ténèbres.");
        AddFR(391, "Robe Rouge de Vesper", "Robe pour magiciens de rêve aspirants.");
        AddFR(392, "Liens d'Âmes de Vesper", "Liens guidant les âmes.");
        AddFR(393, "Robe d'Énergie de Vesper", "Robe crépitant d'énergie.");
        AddFR(394, "Jupe d'Esprit de Vesper", "Jupe tissée d'énergie d'esprit.");
        AddFR(395, "Cape d'Âmes de Vesper", "Cape contenant des âmes.");
        AddFR(396, "Enveloppements d'Âmes de Vesper", "Enveloppements d'âmes capturées.");
        AddFR(397, "Robe de Dragon Arcanum de Vesper", "Robe légendaire du Dragon Arcanum.");
        AddFR(398, "Jambières d'Âmes de Vesper", "Jambières imprégnées d'énergie d'âmes.");
        AddFR(399, "Jambières Exotiques de Vesper", "Jambières exotiques de royaumes lointains.");
        AddFR(400, "Jambières d'Âmes de Vesper", "Jambières guidant la force d'âmes.");
        AddFR(401, "Pantalon Sanguin de Vesper", "Pantalon teint de magie de sang.");
        AddFR(402, "Jambières de Magma de Vesper", "Jambières forgées dans le feu magique.");
        AddFR(403, "Jambières de Sagesse de Vesper", "Jambières de sagesse ancienne.");
        AddFR(404, "Pantalon des Anciens de Vesper", "Pantalon légendaire des Anciens.");
        AddFR(405, "Pantoufles d'Acolyte", "Pantoufles douces portées par les nouveaux membres du temple. Elles portent des prières à chaque pas.");
        AddFR(406, "Chaussures du Temple", "Chaussures traditionnelles des serviteurs d'El. Elles ancrent Vesper dans sa foi.");
        AddFR(407, "Bottes de Moine Étrange", "Bottes portées par les moines étudiant des textes interdits. La connaissance et la foi s'entrelacent.");
        AddFR(408, "Enveloppements Bénis par Nains", "Enveloppements enchantés par des artisans nains. La technologie rencontre la divinité.");
        AddFR(409, "Pantoufles de Soie de Vampire", "Pantoufles élégantes tissées de soie de vampire. Elles tirent la vie de la terre elle-même.");
        AddFR(410, "Pantoufles Tueur de Démons", "Pantoufles vertes bénies pour détruire le mal. Elles brûlent les démons à chaque pas.");
        AddFR(411, "Bottes Chassant les Cauchemars", "Bottes légendaires traversant les cauchemars pour sauver les innocents. Mission ultime de Vesper.");
        
        // Yubar Armor (412-439)
        AddFR(412, "Masque Tribal de Yubar", "Masque pour guerriers tribaux.");
        AddFR(413, "Casque Viking de Yubar", "Casque pour guerriers du nord.");
        AddFR(414, "Casque Cornu de Yubar", "Casque avec cornes terrifiantes.");
        AddFR(415, "Coiffe Ix Endurante de Yubar", "Coiffe pour la tribu Ix.");
        AddFR(416, "Coiffe de Peur de Feu de Yubar", "Coiffe brûlant de peur.");
        AddFR(417, "Casque Ix Endurant de Yubar", "Casque pour guerriers Ix.");
        AddFR(418, "Visage de l'Apocalypse de Yubar", "Visage légendaire de l'Apocalypse.");
        AddFR(419, "Peau d'Ours de Yubar", "Armure d'un puissant ours.");
        AddFR(420, "Cape de Fourrure de Mammouth de Yubar", "Cape d'un mammouth.");
        AddFR(421, "Robe Ix Endurante de Yubar", "Robe pour la tribu Ix.");
        AddFR(422, "Manteau de Magma de Yubar", "Manteau forgé dans la magma.");
        AddFR(423, "Cuirasse Ix Endurante de Yubar", "Cuirasse pour guerriers Ix.");
        AddFR(424, "Armure de Plaques de Lave de Yubar", "Armure de plaques de lave.");
        AddFR(425, "Armure de Géant de Feu de Yubar", "Armure légendaire du Géant de Feu.");
        AddFR(426, "Jambières Ix Endurantes de Yubar", "Jambières pour la tribu Ix.");
        AddFR(427, "Pantalon de Peau Mutée de Yubar", "Pantalon fait de peau mutée.");
        AddFR(428, "Jupe Ix Endurante de Yubar", "Jupe pour guerriers Ix.");
        AddFR(429, "Jambières Naines de Yubar", "Jambières naines solides.");
        AddFR(430, "Jambières d'Alliage de Yubar", "Jambières renforcées d'alliage.");
        AddFR(431, "Jambières de Plaques de Yubar", "Jambières de plaques lourdes.");
        AddFR(432, "Jambières Naines de Yubar", "Jambières légendaires d'artisanat nain.");
        AddFR(433, "Bottes de Combat Temporaires", "Bottes assemblées de fragments de champ de bataille. Yubar fait avec ce qu'il trouve.");
        AddFR(434, "Enveloppements Tribaux", "Enveloppements traditionnels du peuple de Yubar. Ils le relient à ses ancêtres.");
        AddFR(435, "Jambières de Guerrier Cerf", "Bottes ornées de bois de cerf. Elles confèrent la puissance du Roi de la Forêt.");
        AddFR(436, "Bottes du Piétineur de Crocs", "Bottes brutales avec pointes en forme de crocs. Elles écrasent les ennemis sous les pieds.");
        AddFR(437, "Bottes de Berserker Sanguin", "Bottes rouge sang enflammant la rage de Yubar. La douleur devient force.");
        AddFR(438, "Piétineur d'Âmes", "Bottes récoltant les âmes des ennemis tombés. Leur puissance grandit avec chaque bataille.");
        AddFR(439, "Bottes du Maître Retour au Foyer", "Bottes légendaires ramenant spirituellement Yubar à la maison. Ses ancêtres combattent à ses côtés.");
        
        // Legendary Items (440-443)
        AddFR(440, "Casque Ailé du Marcheur de Rêve", "Casque légendaire donnant aux marcheurs de rêve la capacité de voler. Attaque automatiquement les ennemis les plus proches.");
        AddFR(441, "Armure de Plaques de Magie de Rêve", "Armure légendaire protégeant les marcheurs de rêve de tous les royaumes. Vise automatiquement les ennemis les plus proches.");
        AddFR(442, "Jambières des Profondeurs", "Jambières légendaires forgées dans les parties les plus profondes des rêves.");
        AddFR(443, "Bottes de Marche sur l'Eau", "Bottes légendaires permettant au porteur de marcher sur l'eau. Trésor recherché par les marcheurs de rêve de tous les royaumes.");
        
        // Consumables (444-447)
        AddFR(444, "Tonique du Rêveur", "Un simple remède préparé avec des herbes du monde éveillé. Même le rêve le plus faible commence par une goutte d'espoir.");
        AddFR(445, "Essence de Rêverie", "Distillée des souvenirs de paisibles sommeils. Ceux qui la boivent ressentent la chaleur des rêves oubliés sur leurs blessures.");
        AddFR(446, "Vitalité Lucide", "Créée par Smoothie avec de la poussière d'étoiles et des fragments de cauchemar. Le liquide scintille de la lumière de mille étoiles endormies.");
        AddFR(447, "Élixir d'El", "Une boisson sacrée bénie par la lumière d'El. L'ordre de Vesper garde jalousement sa recette, car il peut guérir même les blessures infligées par les cauchemars les plus sombres.");
        
        // Universal Legendary Belt (448)
        AddFR(448, "Ceinture des Rêves Infinis", "Une ceinture légendaire qui lie le porteur au cycle éternel des rêves. Son pouvoir grandit avec chaque cauchemar vaincu.");
        
        // Hero-Specific Legendary Belts (449-457)
        AddFR(449, "Ceinture Tisseuse de Vie d'Aurena", "Une ceinture forgée à partir des recherches volées d'Aurena sur la force vitale. Elle tisse la vie et la mort en équilibre parfait.");
        AddFR(450, "Ceinturon Cristallin de Bismuth", "Une ceinture de cristal pur qui pulse avec la vision de la fille aveugle. Elle voit ce que les yeux ne peuvent pas.");
        AddFR(451, "Ceinture de l'Œil Pourpre de Lacerta", "Une ceinture qui canalise le regard légendaire de Lacerta. Aucune cible n'échappe à son pouvoir vigilant.");
        AddFR(452, "Ceinture de Champion Duelliste de Mist", "La ceinture ultime de la Maison Astrid. L'honneur de Mist est tissé dans chaque fil.");
        AddFR(453, "Ceinture de l'Arbre-Monde de Nachia", "Une ceinture cultivée à partir de l'Arbre-Monde lui-même. La fureur de la nature coule à travers elle.");
        AddFR(454, "Chaîne Inaltérable de Husk", "Une ceinture forgée à partir d'une résolution inaltérable. La détermination de Husk est sa force.");
        AddFR(455, "Ceinture d'Assassin des Ombres de Shell", "Une ceinture de ténèbres parfaites. Les manipulateurs de Shell n'ont jamais pu briser sa volonté.");
        AddFR(456, "Ceinture d'Inquisiteur du Crépuscule de Vesper", "La ceinture de plus haut rang de l'Ordre d'El. Elle juge les cauchemars et bannit les ténèbres.");
        AddFR(457, "Ceinture de Champion Ancestral de Yubar", "Une ceinture qui invoque les ancêtres de Yubar. Leur force coule à travers chaque fibre.");
        
        // Universal Legendary Ring (458)
        AddFR(458, "Anneau des Rêves Éternels", "Un anneau légendaire qui connecte le porteur au royaume infini des rêves. Son pouvoir transcende toutes les frontières.");
        
        // Hero-Specific Legendary Rings (459-467)
        AddFR(459, "Anneau du Grand Sage d'Aurena", "L'anneau du Grand Sage de l'Arcanum Lunaire. Aurena l'a pris comme preuve de leur hypocrisie.");
        AddFR(460, "Anneau Cœur de Gemme de Bismuth", "Un anneau de magie cristalline pure. Il pulse avec l'essence transformée de la fille aveugle.");
        AddFR(461, "Anneau d'Exécuteur Royal de Lacerta", "L'anneau de l'exécuteur d'élite de la Garde Royale. Lacerta l'a gagné à travers d'innombrables batailles.");
        AddFR(462, "Anneau de l'Héritage Astrid de Mist", "L'anneau ancestral de la Maison Astrid. Le lignage de Mist coule à travers son métal.");
        AddFR(463, "Anneau de Gardien de la Forêt de Nachia", "Un anneau béni par les esprits de la forêt de Nachia. Le pouvoir de la nature est son essence.");
        AddFR(464, "Anneau de Volonté Inébranlable de Husk", "Un anneau forgé à partir d'une volonté inaltérable. La détermination de Husk est son pouvoir.");
        AddFR(465, "Anneau d'Ombre Parfaite de Shell", "Un anneau d'obscurité absolue. Les manipulateurs de Shell n'ont jamais pu contrôler son pouvoir.");
        AddFR(466, "Anneau de la Flamme Sacrée de Vesper", "Un anneau contenant l'essence pure de la Flamme Sacrée d'El. Il bannit toute obscurité.");
        AddFR(467, "Anneau de Champion Tribal de Yubar", "Un anneau qui canalise la force des ancêtres de Yubar. Leur fureur renforce chaque coup.");
        
        // Universal Legendary Amulet (468)
        AddFR(468, "Amulette des Rêves Infinis", "Une amulette légendaire qui lie le porteur au cycle éternel des rêves. Son pouvoir transcende toutes les frontières.");
        
        // Hero-Specific Legendary Amulets (469-477)
        AddFR(469, "Amulette Tisseuse de Vie d'Aurena", "Une amulette forgée à partir des recherches volées d'Aurena sur la force vitale. Elle tisse la vie et la mort en équilibre parfait.");
        AddFR(470, "Cœur Cristallin de Bismuth", "Une amulette de cristal pur qui pulse avec la vision de la fille aveugle. Elle voit ce que les yeux ne peuvent pas.");
        AddFR(471, "Amulette de l'Œil Pourpre de Lacerta", "Une amulette qui canalise le regard légendaire de Lacerta. Aucune cible n'échappe à son pouvoir vigilant.");
        AddFR(472, "Amulette de Champion Duelliste de Mist", "L'amulette ultime de la Maison Astrid. L'honneur de Mist est tissé dans chaque gemme.");
        AddFR(473, "Amulette de l'Arbre-Monde de Nachia", "Une amulette cultivée à partir de l'Arbre-Monde lui-même. La fureur de la nature coule à travers elle.");
        AddFR(474, "Amulette Inaltérable de Husk", "Une amulette forgée à partir d'une résolution inaltérable. La détermination de Husk est sa force.");
        AddFR(475, "Amulette d'Assassin des Ombres de Shell", "Une amulette de ténèbres parfaites. Les manipulateurs de Shell n'ont jamais pu briser sa volonté.");
        AddFR(476, "Amulette d'Inquisiteur du Crépuscule de Vesper", "L'amulette de plus haut rang de l'Ordre d'El. Elle juge les cauchemars et bannit les ténèbres.");
        AddFR(477, "Amulette de Champion Ancestral de Yubar", "Une amulette qui invoque les ancêtres de Yubar. Leur force coule à travers chaque gemme.");
    }
    
    private static void AddFR(int id, string name, string desc)
    {
        _nameFR[id] = name;
        _descFR[id] = desc;
    }
    
    // ============================================================
    // SPANISH TRANSLATIONS
    // ============================================================
    private static void InitializeES()
    {
        _nameES = new Dictionary<int, string>();
        _descES = new Dictionary<int, string>();
        
        // Aurena Weapons (1-18)
        AddES(1, "Garras del Iniciado", "Garras simples dadas a nuevos miembros de la Sociedad Arcanum Lunar. Aurena guardó las suyas incluso después de su expulsión.");
        AddES(2, "Garras del Erudito", "Garras afiladas en textos prohibidos. Los arañazos que dejan susurran secretos.");
        AddES(3, "Garras Lunares", "Garras que brillan débilmente bajo la luz de la luna. Canalizan energía lunar en golpes devastadores.");
        AddES(4, "Navajas Arcanum", "Garras inscritas con runas curativas - ahora usadas para desgarrar en lugar de sanar.");
        AddES(5, "Garras del Exiliado", "Forjadas en secreto después de su destierro. Cada corte es un recordatorio de lo que la Sociedad le quitó.");
        AddES(6, "Agarre del Hereje", "Garras que alguna vez pertenecieron a otro sabio expulsado. Su espíritu guía los golpes de Aurena.");
        AddES(7, "Garras Sacrificiales", "Garras que se vuelven más afiladas a medida que la fuerza vital de Aurena mengua. La Sociedad consideró esta investigación demasiado peligrosa.");
        AddES(8, "Garras Ligadas por Escarcha", "Garras congeladas con hielo de la bóveda más profunda de la Sociedad. Su toque adormece carne y espíritu.");
        AddES(9, "Desgarradores Lunares", "Mejorados con encantamientos prohibidos. Cada herida que infligen cura a los aliados de Aurena.");
        AddES(10, "Garras Radiantes", "Garras bendecidas por la luz solar robada. La investigación de Aurena sobre magia de luz enfureció tanto a los cultos del sol como de la luna.");
        AddES(11, "Garras de Brasa", "Garras encendidas con fuerza vital extraída de soñadores dispuestos. Las llamas nunca mueren.");
        AddES(12, "Garras Umbrales", "Garras sumergidas en la esencia de pesadillas. Aurena aprendió que la oscuridad cura lo que la luz no puede.");
        AddES(13, "Segador Glacial", "Garras talladas en hielo eterno. Congelan la sangre de los enemigos y preservan la fuerza vital de Aurena.");
        AddES(14, "Garras del Fénix", "Garras forjadas en llamas de fénix y templadas con la propia sangre de Aurena. Arden con ira inextinguible.");
        AddES(15, "Desgarrador del Vacío", "Garras capaces de desgarrar la realidad misma. La Sociedad teme lo que Aurena descubrirá a continuación.");
        AddES(16, "Garras Celestiales", "Garras impregnadas de pura luz estelar. Cada golpe es una oración por el conocimiento que los dioses quieren ocultar.");
        AddES(17, "Garras del Gran Sabio", "Garras ceremoniales del gran sabio de la Sociedad. Aurena las usa como prueba de su hipocresía.");
        AddES(18, "Garras del Tejedor de Vida", "Obra maestra de Aurena - garras capaces de robar fuerza vital sin destruirla. La Sociedad lo llama herejía.");
        
        // Bismuth Weapons (19-36)
        AddES(19, "Grimorio Vacío", "Libro de hechizos vacío esperando ser llenado. Como Bismuth misma, sus páginas contienen potencial infinito.");
        AddES(20, "Libro del Aprendiz", "Libro de hechizos para principiantes. La chica ciega que se convirtió en Bismuth trazó sus páginas con los dedos, soñando con magia.");
        AddES(21, "Códice Cristalino", "Libro cuyas páginas están hechas de gemas delgadas. Resuena con la naturaleza cristalina de Bismuth.");
        AddES(22, "Diario del Vagabundo", "Diario de viaje registrando observaciones de la chica ciega que nunca vio pero siempre supo.");
        AddES(23, "Introducción a las Gemas", "Guía básica sobre magia cristalina. Sus palabras brillan como el cuerpo arcoíris de Bismuth.");
        AddES(24, "Clásico del Ciego", "Libro escrito en letras en relieve para ciegos. Bismuth lo lee por tacto y memoria.");
        AddES(25, "Libro de las Palabras de Brasa", "Libro cuyas páginas arden eternamente. Las llamas hablan con Bismuth en colores que siente, no ve.");
        AddES(26, "Diccionario Helado", "Libro envuelto en hielo eterno. Sus páginas heladas preservan conocimiento de antes del inicio de los sueños.");
        AddES(27, "Manuscrito Radiante", "Libro que emite luz interior. Antes de que se convirtiera en Bismuth, guió a la chica ciega a través de la oscuridad.");
        AddES(28, "Grimorio de la Sombra", "Libro que absorbe la luz. Sus páginas oscuras contienen secretos que incluso Bismuth teme leer.");
        AddES(29, "Libro de Hechizos Viviente", "Libro consciente que eligió fusionarse con la chica ciega. Juntos se convirtieron en algo más grande.");
        AddES(30, "Códice Prismático", "Libro que descompone la magia en sus colores componentes. Bismuth ve el mundo a través de la luz que refracta.");
        AddES(31, "Crónica del Vacío", "Libro registrando los recuerdos de aquellos perdidos en la oscuridad. Bismuth lee sus historias para honrarlos.");
        AddES(32, "Grimorio del Corazón de Gema", "El libro de hechizos original que se fusionó con la chica ciega. Sus páginas laten con vida cristalina.");
        AddES(33, "Libro de la Luz Cegadora", "Libro tan cegador que puede cegar a los videntes. Para Bismuth, es solo calor.");
        AddES(34, "Anales Ligados por Escarcha", "Libro antiguo congelado en el tiempo. Sus profecías cuentan de una gema que camina como una chica.");
        AddES(35, "Libro Sin Nombre", "Libro sin nombre, como la chica sin vista. Juntos encontraron su identidad.");
        AddES(36, "Clásico del Infierno", "Libro ardiendo con la pasión de un soñador que se niega a dejar que la ceguera lo defina.");
        
        // Lacerta Weapons (37-54)
        AddES(37, "Rifle del Recluta", "Arma estándar entregada a reclutas de la Guardia Real. Lacerta lo dominó antes del final de la primera semana de entrenamiento.");
        AddES(38, "Rifle Largo de Cazador", "Rifle de caza confiable. Lacerta usó uno similar para mantener a su familia antes de unirse a la Guardia.");
        AddES(39, "Carabina de Patrulla", "Compacta y confiable. Perfecta para largas patrullas en las fronteras del reino.");
        AddES(40, "Rifle del Francotirador", "Rifle equilibrado diseñado para quienes valoran la precisión sobre el poder.");
        AddES(41, "Mosquete de Pólvora Negra", "Viejo pero confiable. El olor de la pólvora negra recuerda a Lacerta su primera caza.");
        AddES(42, "Rifle de Explorador", "Rifle ligero para misiones de reconocimiento. La velocidad y el sigilo superan el poder bruto.");
        AddES(43, "Rifle de la Guardia Real", "Arma que Lacerta llevó durante su servicio. Cada arañazo cuenta una historia.");
        AddES(44, "Orgullo del Francotirador", "Rifle preciso para quienes nunca fallan. El ojo escarlata de Lacerta ve lo que otros no pueden.");
        AddES(45, "Rifle Largo Ardiente", "Cargado con balas incendiarias. Los enemigos arden mucho después del impacto.");
        AddES(46, "Carabina Mordedura de Escarcha", "Dispara balas enfriadas al cero absoluto. Los objetivos se ralentizan a paso de caracol antes del golpe mortal.");
        AddES(47, "Rifle Nocturno", "Arma para cazar en la oscuridad. Sus disparos son silenciosos como la sombra.");
        AddES(48, "Mosquete del Amanecer", "Bendecido por la luz del amanecer. Sus disparos atraviesan la oscuridad y el engaño.");
        AddES(49, "Ojo Escarlata", "Nombrado por la mirada legendaria de Lacerta. Ningún objetivo escapa a la vista de este rifle.");
        AddES(50, "Ejecutor Real", "Reservado para la élite de la Guardia. Lacerta recibió este rifle después de salvar el reino.");
        AddES(51, "Cañón Solar", "Rifles que disparan luz concentrada. Cada disparo es un amanecer en miniatura.");
        AddES(52, "Cazador del Vacío", "Rifles que disparan balas de pura oscuridad. Los objetivos desaparecen sin rastro.");
        AddES(53, "Cero Absoluto", "El punto más frío de la existencia. Incluso el tiempo se congela ante él.");
        AddES(54, "Aliento de Dragón", "Rifles cargados con balas explosivas incendiarias. Los enemigos del reino aprendieron a temer su rugido.");
        
        // Mist Weapons (55-70)
        AddES(55, "Florete de Entrenamiento", "Espada de entrenamiento de la juventud de Mist. Incluso entonces superó a cada instructor en Astrid.");
        AddES(56, "Estoque Noble", "Estoque que coincide con el linaje noble de Mist. Ligero, elegante, mortal.");
        AddES(57, "Estoque de Duelista", "Estoque diseñado para combate uno contra uno. Mist nunca perdió un duelo.");
        AddES(58, "Estoque Rápido", "Ligero como una extensión del brazo de Mist.");
        AddES(59, "Estoque de Astrid", "Forjado en las mejores herrerías de Astrid. Símbolo de la artesanía real.");
        AddES(60, "Estoque del Bailarín", "Estoque para quienes tratan el combate como arte. Los movimientos de Mist son poéticos.");
        AddES(61, "Estoque Ardiente", "Estoque rodeado de llamas. La ira de Mist es tan caliente como su determinación.");
        AddES(62, "Estoque de la Sombra", "Estoque que golpea desde la oscuridad. Los enemigos caen antes de ver a Mist moverse.");
        AddES(63, "Estoque Mordedura de Escarcha", "Estoque de acero helado. Su toque adormece cuerpo y espíritu.");
        AddES(64, "Gracia de Camilla", "Regalo de alguien que Mist apreciaba. Ella lucha para honrar su memoria.");
        AddES(65, "Estoque de Parada", "Estoque diseñado para defensa y ataque. Mist convierte cada ataque en oportunidad.");
        AddES(66, "Estoque Radiante", "Estoque que emite luz interior. Atraviesa mentiras y sombras.");
        AddES(67, "Estoque del Infierno", "Estoque ardiendo con celo inextinguible. La determinación de Mist no puede ser apagada.");
        AddES(68, "Estoque de Medianoche", "Estoque forjado en oscuridad absoluta. Golpea con el silencio de la muerte.");
        AddES(69, "Estoque de Invierno", "Estoque tallado en hielo eterno. Su frío solo puede igualarse con la concentración de Mist en combate.");
        AddES(70, "Estoque Indomable", "El estoque legendario del mayor duelista de Astrid. Mist ganó este título con innumerables victorias.");
        
        // Nachia Weapons (71-88)
        AddES(71, "Bastón de Brote", "Rama joven del bosque espiritual. Incluso los brotes responden al llamado de Nachia.");
        AddES(72, "Invocador de Espíritus", "Bastón tallado para guiar las voces de los espíritus del bosque. Susurran secretos a Nachia.");
        AddES(73, "Rama del Bosque", "Rama viva que aún crece. La magia del bosque fluye a través de ella.");
        AddES(74, "Bastón Salvaje", "Salvaje e impredecible, como la propia Nachia. Los espíritus aman su energía caótica.");
        AddES(75, "Bastón del Guardián", "Llevado por quienes protegen el bosque. Nachia nació con esta responsabilidad.");
        AddES(76, "Varita del Mundo Espiritual", "Varita conectando el mundo material y el espiritual. Nachia camina entre ambos.");
        AddES(77, "Bastón de Madera Helada", "Bastón congelado en invierno eterno. Los espíritus fríos del norte responden a su llamado.");
        AddES(78, "Bastón de Raíz de Sombra", "Creciendo en las partes más profundas del bosque. Los espíritus de sombra bailan alrededor.");
        AddES(79, "Bastón de Madera de Brasa", "Bastón que nunca deja de arder. Los espíritus de fuego son atraídos por su calor.");
        AddES(80, "Bastón de Bendición Solar", "Bendecido por espíritus del amanecer. Su luz guía a las almas perdidas a casa.");
        AddES(81, "Colmillos de Fenrir", "Bastón con colmillos de lobo espiritual en la punta. El compañero leal de Nachia guía su poder.");
        AddES(82, "Bastón de Roble Antiguo", "Tallado de un roble milenario. Los espíritus más antiguos recuerdan cuando fue plantado.");
        AddES(83, "Rama del Árbol del Mundo", "Rama del legendario Árbol del Mundo. Todos los espíritus del bosque se inclinan ante él.");
        AddES(84, "Bastón del Rey de los Espíritus", "El bastón del Rey de los Espíritus mismo. Nachia lo ganó protegiendo el bosque.");
        AddES(85, "Bastón de Escarcha Eterna", "Bastón de hielo eterno del corazón helado del bosque. Los espíritus de invierno obedecen sus órdenes.");
        AddES(86, "Percha del Fénix", "Bastón donde anidan los espíritus de fuego. Sus llamas traen renacimiento, no destrucción.");
        AddES(87, "Bastón del Bosquecillo Radiante", "Bastón que emite la luz de mil luciérnagas. Los espíritus de esperanza bailan dentro.");
        AddES(88, "Bastón del Guardián del Vacío", "Bastón guardando las fronteras del mundo. Los espíritus de sombra protegen a su portador.");
        
        // Shell Weapons (89-106)
        AddES(89, "Katana del Novicio", "Katana samurái básico para quienes aprenden el camino de la espada. Shell lo dominó en pocos días.");
        AddES(90, "Katana Silencioso", "Katana samurái diseñado para no hacer ruido. Las víctimas de Shell nunca oyen venir la muerte.");
        AddES(91, "Katana del Asesino", "Delgado y preciso. Shell solo necesita un golpe en el lugar correcto.");
        AddES(92, "Katana Asesino", "Eficiente. Práctico. Despiadado. Como el propio Shell.");
        AddES(93, "Katana del Cazador", "Katana samurái para cazar. Shell no se detiene hasta que el objetivo sea eliminado.");
        AddES(94, "Katana Vacío", "Nombrado por el vacío que Shell siente. O no siente.");
        AddES(95, "Katana del Ejecutor", "Katana samurái que termina innumerables vidas. Shell no siente nada por ninguna de ellas.");
        AddES(96, "Katana de Precisión", "Katana samurái afinado para explotar debilidades. Shell calcula el ataque óptimo.");
        AddES(97, "Katana Mordedura de Escarcha", "Katana samurái de malicia fría. Su frío coincide con el corazón vacío de Shell.");
        AddES(98, "Katana de Purificación", "Katana samurái brillante para trabajo oscuro. Shell no siente la ironía.");
        AddES(99, "Katana de la Sombra", "Katana samurái que bebe sombras. Shell se mueve invisiblemente, golpea silenciosamente.");
        AddES(100, "Daga Ardiente", "Katana samurái calentado por fuego interior. Los objetivos de Shell arden antes de sangrar.");
        AddES(101, "Herramienta del Asesino Perfecto", "La herramienta definitiva del asesino definitivo. Shell nació para esta espada.");
        AddES(102, "Hoja Despiadada", "Katana samurái que nunca se desafila, blandido por un perseguidor incansable.");
        AddES(103, "Asesino del Infierno", "Katana samurái forjado en fuego infernal. Arde con la intensidad del único propósito de Shell.");
        AddES(104, "Katana del Vacío", "Katana samurái de oscuridad absoluta. Como Shell, existe solo para terminar.");
        AddES(105, "Katana Helado", "Katana samurái de hielo eterno. Su juicio es tan frío y final como Shell.");
        AddES(106, "Fin Radiante", "La forma definitiva del katana samurái brillante. Shell ejecuta juicios en nombre de El.");
        
        // Vesper Weapons (107-124)
        AddES(107, "Cetro del Novicio", "Cetro simple para nuevos miembros de la Orden de la Llama Solar. Vesper comenzó su viaje aquí.");
        AddES(108, "Martillo del Juicio", "Martillo de guerra usado para ejecutar la voluntad de El. Cada golpe es un juicio sagrado.");
        AddES(109, "Bastón del Guardián del Fuego", "Arma llevada por quienes guardan el fuego sagrado. Sus golpes purifican la impureza.");
        AddES(110, "Escudo del Acólito", "Escudo simple para acólitos de la Orden. La fe es la mejor defensa.");
        AddES(111, "Escudo del Inquisidor", "Escudo llevado por inquisidores. Ha visto la caída de innumerables herejes.");
        AddES(112, "Defensor del Crepúsculo", "Escudo guardando entre luz y oscuridad.");
        AddES(113, "Martillo del Fanático", "Martillo de guerra ardiendo con fanatismo religioso. Los enemigos tiemblan ante él.");
        AddES(114, "Martillo Pesado de Purificación", "Martillo de guerra enorme usado para purificar el mal. Un golpe aplasta el pecado.");
        AddES(115, "Juicio de El", "Martillo de guerra bendecido personalmente por El. Su juicio es inapelable.");
        AddES(116, "Bastión de la Fe", "Escudo indestructible. Mientras la fe no se extinga, no se romperá.");
        AddES(117, "Escudo Sagrado de Llama", "Escudo rodeado de fuego sagrado. El mal que lo toca es quemado.");
        AddES(118, "Bastión de la Inquisición", "Escudo de un alto inquisidor. Vesper lo ganó con servicio despiadado.");
        AddES(119, "Rompedor del Amanecer", "Martillo de guerra legendario aplastando la oscuridad. Vesper blande la encarnación de la ira de El.");
        AddES(120, "Destructor Sagrado", "Martillo de guerra bendecido por el sumo sacerdote. Sus golpes llevan el peso de la ira sagrada.");
        AddES(121, "Mano Derecha de El", "El arma más sagrada de la Orden. Vesper es la herramienta elegida de la voluntad de El.");
        AddES(122, "Fortaleza de Llama", "La defensa definitiva de la Orden. La luz de El protege a Vesper de todo daño.");
        AddES(123, "Vigilancia Eterna", "Escudo inquebrantable. Como Vesper, siempre vigila sobre la oscuridad.");
        AddES(124, "Juramento del Paladín del Crepúsculo", "Escudo del líder de la Orden. Vesper juró vincular su alma a El.");
        
        // Yubar Weapons (125-142)
        AddES(125, "Estrella Recién Nacida", "Estrella que acaba de formarse. Yubar recuerda cuando todas las estrellas eran tan pequeñas.");
        AddES(126, "Brasa Cósmica", "Fragmento de fuego estelar. Arde con el calor de la creación misma.");
        AddES(127, "Esfera de Polvo Estelar", "Polvo estelar comprimido esperando ser moldeado. Yubar teje galaxias con tal materia.");
        AddES(128, "Catalizador de Sueño", "Esfera que amplifica la energía de los sueños. Proveniente de los primeros experimentos de Yubar.");
        AddES(129, "Núcleo de Nebulosa", "El corazón de una nebulosa distante. Yubar lo arrancó del tapiz cósmico.");
        AddES(130, "Semilla Celestial", "Semilla que se convertirá en estrella. Yubar cultiva innumerables semillas así.");
        AddES(131, "Fragmento de Supernova", "Fragmento de una estrella explotada. Yubar presenció su muerte y preservó su poder.");
        AddES(132, "Pozo Gravitacional", "Esfera que comprime la gravedad. El espacio mismo se curva alrededor.");
        AddES(133, "Singularidad del Vacío", "Punto de densidad infinita. Yubar lo usa para crear y destruir mundos.");
        AddES(134, "Corona Solar", "Fragmento de la atmósfera exterior del sol. Arde con la ira de la estrella.");
        AddES(135, "Cometa Helado", "Núcleo de cometa capturado. Lleva secretos de las profundidades del cosmos.");
        AddES(136, "Forja Estelar", "Miniatura de un núcleo estelar. Yubar la usa para forjar nueva realidad.");
        AddES(137, "Reliquia del Big Bang", "Fragmento del Big Bang de la creación. Contiene el origen del universo.");
        AddES(138, "Telar Cósmico", "Herramienta que teje realidad. Yubar la usa para remodelar el destino.");
        AddES(139, "Creación Radiante", "La luz de la creación misma. Yubar la usó para dar a luz a las primeras estrellas.");
        AddES(140, "Corazón de la Entropía", "La esencia de la descomposición cósmica. Todo termina, y Yubar decide cuándo.");
        AddES(141, "Cero Absoluto", "El punto más frío de la existencia. Incluso el tiempo se congela ante él.");
        AddES(142, "Núcleo de Catástrofe", "El corazón de la destrucción cósmica. Yubar solo lo libera en momentos críticos.");
        
        // Rings (143-162)
        AddES(143, "Anillo del Soñador", "Anillo simple llevado por quienes acaban de entrar al mundo de los sueños. Emite débil esperanza.");
        AddES(144, "Anillo de Polvo Estelar", "Forjado de polvo estelar condensado. Los soñadores dicen que brilla más fuerte en peligro.");
        AddES(145, "Marca del Sueño", "Grabado de un sueño pacífico. Los portadores se recuperan más rápido de las heridas.");
        AddES(146, "Anillo de Espíritu", "Anillo que captura espíritus errantes. Su luz guía caminos a través de sueños oscuros.");
        AddES(147, "Anillo del Vagabundo", "Llevado por vagabundos de los reinos de los sueños. Ha visto innumerables caminos olvidados.");
        AddES(148, "Anillo de Fragmento de Pesadilla", "Anillo que contiene fragmentos de pesadillas derrotadas. Su oscuridad fortalece a los valientes.");
        AddES(149, "Guardián de la Memoria", "Anillo que preserva recuerdos preciosos. Smoothie vende similares en su tienda.");
        AddES(150, "Anillo del Arcanum Lunar", "Anillo de la Sociedad Arcanum Lunar. Aurena llevó uno similar antes de su exilio.");
        AddES(151, "Marca del Bosque Espiritual", "Anillo bendecido por los espíritus del bosque de Nachia. La fuerza de la naturaleza fluye a través.");
        AddES(152, "Insignia de la Guardia Real", "Anillo que Lacerta llevó durante su servicio. Todavía lleva el peso de la responsabilidad.");
        AddES(153, "Gloria del Duelista", "Anillo pasado entre duelistas nobles. La familia de Mist aprecia tales anillos.");
        AddES(154, "Amuleto de Brasa de Llama", "Chispa sellada del fuego sagrado. La Orden de Vesper guarda estas reliquias.");
        AddES(155, "Fragmento de Libro de Hechizos", "Anillo hecho de magia cristalina de Bismuth. Zumba con energía arcanum.");
        AddES(156, "Marca del Cazador", "Anillo que rastrea presa. Los controladores de Shell los usan para monitorear sus herramientas.");
        AddES(157, "Bendición de El", "Anillo bendecido por El mismo. Su luz disipa pesadillas y cura a los heridos.");
        AddES(158, "Marca del Señor de Pesadillas", "Tomado de un señor de pesadillas derrotado. Su poder oscuro corrompe y fortalece.");
        AddES(159, "Legado de Astrid", "Anillo de la casa noble de Astrid. Los ancestros de Mist lo llevaron en innumerables duelos.");
        AddES(160, "Ojo del Vidente", "Anillo capaz de ver a través del velo de los sueños. Pasado, presente y futuro se mezclan.");
        AddES(161, "Anillo del Sueño Primordial", "Forjado en el amanecer de los sueños. Contiene la esencia del primer soñador.");
        AddES(162, "Sueño Eterno", "Anillo que toca el sueño más profundo. Los portadores no temen ni la muerte ni el despertar.");
        
        // Amulets (163-183)
        AddES(163, "Colgante del Soñador", "Colgante simple llevado por quienes acaban de entrar al mundo de los sueños. Emite débil esperanza.");
        AddES(164, "Colgante del Toque de Fuego", "Colgante tocado por el fuego. Su calor disipa frío y miedo.");
        AddES(165, "Lágrima Escarlata", "Rubí en forma de lágrima de sangre. Su poder viene del sacrificio.");
        AddES(166, "Amuleto de Ámbar", "Ámbar que contiene una criatura antigua. Sus recuerdos fortalecen al portador.");
        AddES(167, "Medalla del Crepúsculo", "Medalla parpadeando entre luz y oscuridad. Los nuevos miembros de Vesper llevan estas.");
        AddES(168, "Collar de Polvo Estelar", "Collar espolvoreado con estrellas cristalinas. Smoothie las recolecta de sueños caídos.");
        AddES(169, "Ojo de Esmeralda", "Gema verde capaz de ver a través de ilusiones. Las pesadillas no pueden escapar de su mirada.");
        AddES(170, "Corazón del Espíritu del Bosque", "Gema que contiene la bendición de los espíritus del bosque. Los guardianes de Nachia aprecian estas.");
        AddES(171, "Marca del Arcanum Lunar", "Marca de archivos prohibidos. Aurena arriesga todo estudiándolas.");
        AddES(172, "Cristal de Bismuth", "Fragmento de magia pura cristalina. Resuena con energía arcanum.");
        AddES(173, "Amuleto de Brasa de Llama", "Chispa sellada del fuego sagrado. La Orden de Vesper guarda estas reliquias.");
        AddES(174, "Colgante de Colmillo de Pesadilla", "Colmillo de una pesadilla poderosa, ahora un trofeo. Shell lleva uno como recordatorio.");
        AddES(175, "Medalla de la Guardia Real", "Medalla de honor de la Guardia Real. Lacerta ganó muchas antes de su exilio.");
        AddES(176, "Escudo de Armas de Astrid", "Escudo de armas noble de la familia de Astrid. El legado de la familia de Mist cuelga de esta cadena.");
        AddES(177, "Lágrima Sagrada de El", "Lágrima derramada por El mismo. Su luz protege contra las pesadillas más oscuras.");
        AddES(178, "Esmeralda Primordial", "Gema del primer bosque. Contiene los sueños de espíritus antiguos.");
        AddES(179, "Ojo del Señor de Pesadillas", "Ojo de un señor de pesadillas derrotado. Ve todos los miedos y los explota.");
        AddES(180, "Corazón de la Llama", "El núcleo del fuego sagrado mismo. Solo los más piadosos pueden llevarlo.");
        AddES(181, "Oráculo del Vidente", "Amuleto capaz de ver todos los futuros posibles. La realidad se curva ante sus profecías.");
        AddES(182, "Brújula del Viajero del Vacío", "Amuleto que apunta al vacío. Quienes lo siguen nunca regresan de la misma manera.");
        AddES(183, "Amuleto de Midas", "Amuleto legendario tocado por el sueño del oro. Los enemigos derrotados dejan caer oro adicional.");
        AddES(478, "Recolector de Polvo Estelar", "Amuleto legendario que recoge polvo estelar cristalizado de enemigos derrotados. Cada eliminación produce esencia de sueño preciosa (base 1-5, escala con nivel de mejora y fuerza del monstruo).");
        
        // Belts (184-203)
        AddES(184, "Cinturón del Vagabundo", "Cinturón de tela simple llevado por quienes vagan entre sueños. Ancla el alma.");
        AddES(185, "Cuerda del Soñador", "Cuerda tejida de hilos de sueño. Pulsa suavemente con cada latido del corazón.");
        AddES(186, "Cinturón de Polvo Estelar", "Cinturón adornado con polvo estelar cristalino. Smoothie vende accesorios similares.");
        AddES(187, "Cinturón de la Centinela", "Cinturón sólido llevado por guardianes de los sueños. Nunca se afloja, nunca falla.");
        AddES(188, "Cinturón Místico", "Cinturón impregnado de magia débil. Pincha cuando las pesadillas se acercan.");
        AddES(189, "Cinturón del Cazador", "Cinturón de cuero con bolsas de suministros. Esencial para largos viajes en sueños.");
        AddES(190, "Cinturón del Peregrino", "Llevado por quienes buscan la luz de El. Ofrece consuelo en los sueños más oscuros.");
        AddES(191, "Cinturón del Cazador de Pesadillas", "Cinturón hecho de pesadillas derrotadas. Su esencia fortalece al portador.");
        AddES(192, "Cinturón Arcanum", "Cinturón de la Sociedad Arcanum Lunar. Amplifica energía mágica.");
        AddES(193, "Cinturón de Llama", "Cinturón bendecido por la Orden de Vesper. Arde con fuego de protección.");
        AddES(194, "Cinturón de Espada del Duelista", "Cinturón elegante llevado por guerreros nobles de Astrid. Lleva espadas y honor.");
        AddES(195, "Cuerda del Tejedor de Espíritus", "Cinturón tejido por espíritus del bosque para Nachia. Zumba con energía natural.");
        AddES(196, "Cinturón de Herramientas del Asesino", "Cinturón con compartimentos ocultos. Los controladores de Shell los usan para equipar sus herramientas.");
        AddES(197, "Cinturón de Pistolera", "Cinturón diseñado para las armas de fuego de Lacerta. Desenfundado rápido, muerte más rápida.");
        AddES(198, "Cinturón Sagrado de El", "Cinturón bendecido por El mismo. Su luz ancla el alma contra toda oscuridad.");
        AddES(199, "Cadena del Señor de Pesadillas", "Cinturón forjado de las cadenas de un señor de pesadillas. Vincula el miedo a la voluntad del portador.");
        AddES(200, "Cinturón del Sueño Primordial", "Cinturón del primer sueño. Contiene el eco de la creación misma.");
        AddES(201, "Cinturón de Raíz del Árbol del Mundo", "Cinturón que crece de las raíces del Árbol del Mundo. El bosque de Nachia bendice su creación.");
        AddES(202, "Legado de Astrid", "Cinturón familiar de la familia de Astrid. La sangre de Mist está tejida en su tela.");
        AddES(203, "Cinturón del Inquisidor del Crepúsculo", "Cinturón del más alto nivel de Vesper. Juzga a todos los que se paran ante él.");
        
        // Armor Sets - Bone Sentinel (204-215)
        AddES(204, "Máscara de la Centinela de Hueso", "Máscara tallada del cráneo de una pesadilla caída. Sus ojos vacíos ven a través de ilusiones.");
        AddES(205, "Casco del Guardián del Mundo Espiritual", "Casco impregnado de espíritus del bosque de Nachia. Susurros de protección rodean al portador.");
        AddES(206, "Corona del Señor de Pesadillas", "Corona forjada de pesadillas conquistadas. El portador gobierna sobre el miedo mismo.");
        AddES(207, "Chaleco de la Centinela de Hueso", "Armadura de las costillas de criaturas que murieron en sueños. Su protección dura más allá de la muerte.");
        AddES(208, "Coraza del Guardián del Mundo Espiritual", "Coraza tejida de sueños cristalizados. Se mueve y se adapta a los golpes entrantes.");
        AddES(209, "Coraza del Señor de Pesadillas", "Armadura pulsando con energía oscura. Las pesadillas se inclinan ante su portador.");
        AddES(210, "Grebas de la Centinela de Hueso", "Grebas reforzadas con huesos de pesadilla. Nunca se cansan, nunca ceden.");
        AddES(211, "Grebas del Guardián del Mundo Espiritual", "Grebas bendecidas por espíritus del bosque. El portador se mueve como viento a través de hojas.");
        AddES(212, "Grebas del Señor de Pesadillas", "Grebas que beben sombras. La oscuridad fortalece cada paso.");
        AddES(213, "Botas de la Centinela de Hueso", "Botas para caminar silenciosamente en los reinos de los sueños. Los muertos no hacen ruido.");
        AddES(214, "Botas de Hierro del Guardián del Mundo Espiritual", "Botas que no dejan rastro en los reinos de los sueños. Perfectas para cazadores de pesadillas.");
        AddES(215, "Botas del Señor de Pesadillas", "Botas que cruzan mundos. La realidad se curva bajo cada paso.");
        
        // Aurena Armor (216-243)
        AddES(216, "Sombrero de Aprendiz de Aurena", "Sombrero simple llevado por novicios del Arcanum Lunar.");
        AddES(217, "Turbante Místico de Aurena", "Turbante impregnado de débil energía lunar.");
        AddES(218, "Corona Encantadora de Aurena", "Corona que mejora la afinidad mágica del portador.");
        AddES(219, "Corona Antigua de Aurena", "Reliquia antigua transmitida a través de generaciones.");
        AddES(220, "Banda de Halcón de Aurena", "Banda bendecida por espíritus del cielo.");
        AddES(221, "Corona de Poder de Aurena", "Corona poderosa que emite poder arcanum.");
        AddES(222, "Corona Lunar de Aurena", "Corona legendaria de un maestro del Arcanum Lunar.");
        AddES(223, "Túnica Azul de Aurena", "Túnica simple llevada por magos lunares.");
        AddES(224, "Túnica de Mago de Aurena", "Túnica apreciada por magos practicantes.");
        AddES(225, "Túnica Tejida con Hechizos de Aurena", "Túnica tejida con hilos mágicos.");
        AddES(226, "Túnica Iluminadora de Aurena", "Túnica que guía la sabiduría lunar.");
        AddES(227, "Sudario de Sueño de Aurena", "Sudario tejido de sueños cristalizados.");
        AddES(228, "Túnica Real de Escamas de Aurena", "Túnica adecuada para la familia real lunar.");
        AddES(229, "Túnica de la Reina de Hielo de Aurena", "Túnica legendaria de la Reina de Hielo misma.");
        AddES(230, "Grebas Azules de Aurena", "Grebas simples llevadas por magos aspirantes.");
        AddES(231, "Falda de Fibra de Aurena", "Falda ligera que permite movimiento libre.");
        AddES(232, "Grebas Élficas de Aurena", "Grebas elegantes diseñadas por elfos.");
        AddES(233, "Grebas Iluminadoras de Aurena", "Grebas bendecidas por la sabiduría.");
        AddES(234, "Falda Glacial de Aurena", "Falda impregnada de magia de escarcha.");
        AddES(235, "Pantalones de Escarcha de Aurena", "Pantalones que emiten fuerza de frío.");
        AddES(236, "Grebas Magníficas de Aurena", "Grebas legendarias de elegancia incomparable.");
        AddES(237, "Zapatillas Suaves de Luna", "Zapatos suaves llevados por acólitos del Arcanum Lunar. Amortiguan cada paso con luz lunar.");
        AddES(238, "Sandalias del Peregrino", "Sandalias simples bendecidas por los ancianos de la Sociedad. Aurena aún recuerda su calor.");
        AddES(239, "Zapatos de Mística Oriental", "Zapatos exóticos de tierras lejanas. Guían energía curativa a través de la tierra.");
        AddES(240, "Gracia del Caminante de Sueños", "Botas que caminan entre el mundo despierto y el mundo de los sueños. Perfectas para sanadores buscando almas perdidas.");
        AddES(241, "Botas del Caminante de Almas", "Botas impregnadas de la esencia de sanadores fallecidos. Su sabiduría guía cada paso.");
        AddES(242, "Botas del Rastreador de Almas", "Botas oscuras que rastrean a los heridos. Aurena las usa para encontrar a quienes necesitan ayuda... o quienes la lastimaron.");
        AddES(243, "Alas del Sabio Caído", "Botas legendarias que supuestamente otorgan la capacidad de volar a corazones puros. El exilio de Aurena probó su calificación.");
        
        // Bismuth Armor (244-271)
        AddES(244, "Sombrero Cónico de Bismuth", "Sombrero simple para aprendices cristalinos.");
        AddES(245, "Sombrero de Esmeralda de Bismuth", "Sombrero incrustado con cristales de esmeralda.");
        AddES(246, "Máscara Glacial de Bismuth", "Máscara hecha de cristales helados.");
        AddES(247, "Máscara Alarharim de Bismuth", "Máscara antigua de poder cristalino.");
        AddES(248, "Casco Prismático de Bismuth", "Casco que refracta la luz en arcoíris.");
        AddES(249, "Corona Reflectante de Bismuth", "Corona que refleja energía mágica.");
        AddES(250, "Corona Incandescente de Bismuth", "Corona legendaria ardiendo con fuego cristalino.");
        AddES(251, "Túnica de Hielo de Bismuth", "Túnica enfriada por magia cristalina.");
        AddES(252, "Túnica de Monje de Bismuth", "Túnica simple para meditación.");
        AddES(253, "Túnica Glacial de Bismuth", "Túnica congelada en escarcha eterna.");
        AddES(254, "Armadura Cristalina de Bismuth", "Armadura formada de cristales vivientes.");
        AddES(255, "Armadura Prismática de Bismuth", "Armadura que curva la luz misma.");
        AddES(256, "Armadura de Placas Helada de Bismuth", "Armadura de placas de escarcha eterna.");
        AddES(257, "Armadura de Placas Sagrada de Bismuth", "Armadura legendaria bendecida por dioses cristalinos.");
        AddES(258, "Grebas de Cota de Mallas de Bismuth", "Grebas básicas con eslabones cristalinos.");
        AddES(259, "Grebas de Fibra de Bismuth", "Grebas ligeras para magos cristalinos.");
        AddES(260, "Grebas de Esmeralda de Bismuth", "Grebas adornadas con cristales de esmeralda.");
        AddES(261, "Pantalones Extraños de Bismuth", "Pantalones pulsando con energía extraña.");
        AddES(262, "Grebas Prismáticas de Bismuth", "Grebas brillando con luz prismática.");
        AddES(263, "Grebas Alarharim de Bismuth", "Grebas antiguas de Alarharim.");
        AddES(264, "Grebas de Halcón de Bismuth", "Grebas legendarias de la Orden del Halcón.");
        AddES(265, "Zapatillas del Erudito", "Zapatos cómodos para trabajo largo en bibliotecas. El conocimiento fluye a través de sus suelas.");
        AddES(266, "Envoltorios Alarharim", "Envoltorios antiguos de una civilización perdida. Susurran hechizos olvidados.");
        AddES(267, "Zapatos de Hielo", "Botas talladas de hielo eterno. Bismuth deja patrones de escarcha donde camina.");
        AddES(268, "Paso de Flor de Escarcha", "Botas elegantes floreciendo con cristales de hielo. Cada paso crea un jardín de escarcha.");
        AddES(269, "Botas de Resonancia Cristalina", "Botas que amplifican energía mágica. Los cristales zumban con poder sin explotar.");
        AddES(270, "Botas Arcanum Prismáticas", "Botas que refractan la luz en energía mágica pura. La realidad se curva alrededor de cada paso.");
        AddES(271, "Botas del Caminante del Vacío", "Botas legendarias que cruzan el vacío mismo. Bismuth ve caminos que otros no pueden imaginar.");
        
        // Lacerta Armor (272-299)
        AddES(272, "Casco de Cuero de Lagarto", "Casco básico para guerreros dragón.");
        AddES(273, "Casco de Cota de Mallas de Lagarto", "Casco sólido de cota de mallas.");
        AddES(274, "Casco de Guerrero de Lagarto", "Casco llevado por guerreros experimentados.");
        AddES(275, "Casco de Escamas de Dragón de Lagarto", "Casco forjado de escamas de dragón.");
        AddES(276, "Casco Real de Lagarto", "Casco adecuado para la familia real dragón.");
        AddES(277, "Casco de Élite Hombre-Dragón de Lagarto", "Casco de élite de la Orden Hombre-Dragón.");
        AddES(278, "Casco de Oro de Lagarto", "Casco de oro legendario del Rey Dragón.");
        AddES(279, "Armadura de Cuero de Lagarto", "Armadura básica para guerreros dragón.");
        AddES(280, "Armadura de Escamas de Lagarto", "Armadura reforzada con escamas.");
        AddES(281, "Armadura de Caballero de Lagarto", "Armadura pesada para caballeros dragón.");
        AddES(282, "Cota de Mallas de Escamas de Dragón de Lagarto", "Cota de mallas forjada de escamas de dragón.");
        AddES(283, "Cota de Mallas Real Hombre-Dragón de Lagarto", "Cota de mallas real de la Orden Hombre-Dragón.");
        AddES(284, "Armadura de Placas de Halcón de Lagarto", "Armadura de placas de la Orden del Halcón.");
        AddES(285, "Armadura de Oro de Lagarto", "Armadura de oro legendaria del Rey Dragón.");
        AddES(286, "Grebas de Cuero de Lagarto", "Grebas básicas para guerreros dragón.");
        AddES(287, "Grebas Remachadas de Lagarto", "Grebas reforzadas con remaches.");
        AddES(288, "Grebas de Caballero de Lagarto", "Grebas pesadas para caballeros dragón.");
        AddES(289, "Grebas de Escamas de Dragón de Lagarto", "Grebas forjadas de escamas de dragón.");
        AddES(290, "Grebas Reales de Lagarto", "Grebas adecuadas para la familia real.");
        AddES(291, "Grebas Magníficas de Lagarto", "Grebas magníficas de artesanía maestra.");
        AddES(292, "Grebas de Oro de Lagarto", "Grebas de oro legendarias del Rey Dragón.");
        AddES(293, "Botas de Cuero Desgastadas", "Botas simples que han visto muchas batallas. Llevan el aroma de la aventura.");
        AddES(294, "Botas de Combate Parcheadas", "Botas reparadas innumerables veces. Cada parche cuenta una historia de supervivencia.");
        AddES(295, "Botas del Guerrero de Acero", "Botas pesadas forjadas para combate. Anclan a Lacerta al suelo durante la batalla.");
        AddES(296, "Grebas del Guardián", "Botas llevadas por guardianes de élite. Nunca retroceden, nunca se rinden.");
        AddES(297, "Grebas de Escamas de Dragón", "Botas hechas de escamas de dragón. Otorgan al portador la resistencia del dragón.");
        AddES(298, "Botas del Señor de la Guerra Hombre-Dragón", "Botas de antiguos generales hombre-dragón. Temibles en cualquier campo de batalla.");
        AddES(299, "Botas del Maestro de Oro", "Botas legendarias del mayor guerrero de una era. El destino de Lacerta espera.");
        
        // Mist Armor (300-327)
        AddES(300, "Turbante de Mist", "Turbante simple que oculta la identidad.");
        AddES(301, "Turbante Ligero de Mist", "Turbante ligero y transpirable.");
        AddES(302, "Turbante de Visión Nocturna de Mist", "Turbante que mejora la visión nocturna.");
        AddES(303, "Banda de Relámpago de Mist", "Banda cargada con energía eléctrica.");
        AddES(304, "Capucha de Cobra de Mist", "Capucha mortal como una serpiente.");
        AddES(305, "Máscara del Rastreador del Infierno de Mist", "Máscara que rastrea presa.");
        AddES(306, "Susurrador Oscuro de Mist", "Máscara legendaria que susurra muerte.");
        AddES(307, "Arnés de Cuero de Mist", "Arnés ligero para agilidad.");
        AddES(308, "Capa de Guardabosques de Mist", "Capa para movimiento rápido.");
        AddES(309, "Túnica de Medianoche de Mist", "Túnica para golpes nocturnos.");
        AddES(310, "Túnica de Relámpago de Mist", "Túnica cargada con relámpagos.");
        AddES(311, "Armadura de Tensión de Mist", "Armadura pulsando con energía eléctrica.");
        AddES(312, "Armadura de Maestro Arquero de Mist", "Armadura de un maestro arquero.");
        AddES(313, "Capa del Señor Oscuro de Mist", "Capa legendaria del Señor Oscuro.");
        AddES(314, "Grebas de Guardabosques de Mist", "Grebas llevadas por guardabosques.");
        AddES(315, "Grebas de Supervivencia de Jungla de Mist", "Grebas de supervivencia de jungla.");
        AddES(316, "Sarong de Medianoche de Mist", "Sarong para operaciones nocturnas.");
        AddES(317, "Grebas de Relámpago de Mist", "Grebas cargadas con relámpagos.");
        AddES(318, "Grebas de Saltamontes de Mist", "Grebas para saltos asombrosos.");
        AddES(319, "Grebas de Demonio Verde de Mist", "Grebas de velocidad demoníaca.");
        AddES(320, "Grebas del Caminante de Almas de Mist", "Grebas legendarias que cruzan mundos.");
        AddES(321, "Botas de Explorador Temporales", "Botas ensambladas de fragmentos. Ligeras y silenciosas, perfectas para exploración.");
        AddES(322, "Botas del Corredor de Pantano", "Botas impermeables para terreno difícil. Mist conoce cada atajo.");
        AddES(323, "Caminante Rápido", "Botas encantadas que aceleran los pasos del portador. Perfectas para táctica de golpear y correr.");
        AddES(324, "Botas del Cazador de Ciervo", "Botas que rastrean presa.");
        AddES(325, "Botas de Golpe de Relámpago", "Botas cargadas con energía de relámpago.");
        AddES(326, "Botas de Combate del Caminante de Fuego", "Botas capaces de caminar a través del fuego.");
        AddES(327, "Pisoteador de Sufrimiento", "Botas legendarias que pisotean todo sufrimiento.");
        
        // Nachia Armor (328-355)
        AddES(328, "Sombrero de Piel de Nachia", "Sombrero simple para cazadores del bosque.");
        AddES(329, "Tocado de Plumas de Nachia", "Tocado adornado con plumas.");
        AddES(330, "Máscara de Chamán de Nachia", "Máscara de poder espiritual.");
        AddES(331, "Capucha de Tierra de Nachia", "Capucha bendecida por espíritus de la tierra.");
        AddES(332, "Casco de Naturaleza de Nachia", "Casco impregnado de fuerza natural.");
        AddES(333, "Corona de Árbol de Nachia", "Corona que crece de un árbol antiguo.");
        AddES(334, "Corona de Hoja de Nachia", "Corona legendaria del Guardián del Bosque.");
        AddES(335, "Armadura de Piel de Nachia", "Armadura hecha de criaturas del bosque.");
        AddES(336, "Armadura Indígena de Nachia", "Armadura tradicional de las tribus del bosque.");
        AddES(337, "Manto de Madera Verde de Nachia", "Manto tejido de enredaderas vivientes.");
        AddES(338, "Capa de Tierra de Nachia", "Capa bendecida por espíritus de la tierra.");
        AddES(339, "Abrazo de Naturaleza de Nachia", "Armadura unida con la naturaleza.");
        AddES(340, "Armadura de Nido de Pantano de Nachia", "Armadura de pantanos profundos.");
        AddES(341, "Túnica de Hoja de Nachia", "Túnica legendaria del Guardián del Bosque.");
        AddES(342, "Shorts de Piel de Mamut de Nachia", "Shorts cálidos hechos de piel de mamut.");
        AddES(343, "Shorts de Cuero de Nachia", "Grebas de caza tradicionales.");
        AddES(344, "Grebas de Ciervo de Nachia", "Grebas hechas de piel de ciervo.");
        AddES(345, "Grebas de Tierra de Nachia", "Grebas bendecidas por espíritus de la tierra.");
        AddES(346, "Grebas de Hoja de Nachia", "Grebas tejidas de hojas mágicas.");
        AddES(347, "Taparrabo de Hombre-Jabalí de Nachia", "Taparrabo de un poderoso hombre-jabalí.");
        AddES(348, "Grebas de Caza Sangrienta", "Grebas legendarias de caza sangrienta.");
        AddES(349, "Botas Forradas de Piel", "Botas cálidas y cómodas.");
        AddES(350, "Botas de Piel de Tejón", "Botas hechas de piel de tejón.");
        AddES(351, "Botas de Golpe de Cobra", "Botas rápidas como una serpiente.");
        AddES(352, "Envoltorios del Acechador del Bosque", "Envoltorios silenciosos en el bosque.");
        AddES(353, "Botas del Cazador de Tierra", "Botas que rastrean presa.");
        AddES(354, "Botas de Guardabosques de Flor de Fiebre", "Botas adornadas con flores de fiebre. Otorgan velocidad febril y precisión mortal.");
        AddES(355, "Botas del Depredador Sangriento", "Botas legendarias empapadas de la sangre de innumerables cacerías. Nachia se convirtió en el depredador supremo.");
        
        // Shell Armor (356-383)
        AddES(356, "Casco Dañado de Shell", "Casco dañado del mundo subterráneo.");
        AddES(357, "Máscara Rota de Shell", "Máscara rota que aún ofrece protección.");
        AddES(358, "Casco del Señor de Huesos de Shell", "Casco forjado de los restos del Señor de Huesos.");
        AddES(359, "Casco de Esqueleto de Shell", "Casco formado de huesos.");
        AddES(360, "Casco de Muerte de Shell", "Casco de la muerte misma.");
        AddES(361, "Guardián de Esqueleto Nokferatu de Shell", "Casco de huesos de vampiro.");
        AddES(362, "Casco Demoníaco de Shell", "Casco legendario del Señor Demonio.");
        AddES(363, "Sudario Funerario de Shell", "Sudario de la tumba.");
        AddES(364, "Capa Antigua de Shell", "Capa antigua de tiempos antiguos.");
        AddES(365, "Capa de Huesos Nokferatu de Shell", "Capa tejida de huesos.");
        AddES(366, "Capa de Almas de Shell", "Capa de almas inquietas.");
        AddES(367, "Túnica de Muerte de Shell", "Túnica de la muerte.");
        AddES(368, "Túnica del Mundo Subterráneo de Shell", "Túnica de las profundidades.");
        AddES(369, "Armadura Demoníaca de Shell", "Armadura legendaria del Señor Demonio.");
        AddES(370, "Delantal Dañado de Shell", "Delantal dañado pero útil.");
        AddES(371, "Delantal de Huesos Mutados de Shell", "Delantal hecho de huesos mutados.");
        AddES(372, "Envoltorios de Espinas Nokferatu de Shell", "Envoltorios con espinas.");
        AddES(373, "Guardián de Carne Nokferatu de Shell", "Protección hecha de carne conservada.");
        AddES(374, "Grebas Sangrientas de Shell", "Grebas empapadas de sangre.");
        AddES(375, "Caminante de Sangre Nokferatu de Shell", "Grebas que caminan en sangre.");
        AddES(376, "Grebas Demoníacas de Shell", "Grebas legendarias del Señor Demonio.");
        AddES(377, "Grebas Blindadas", "Botas metálicas pesadas que aplastan todo bajo los pies. La defensa es el mejor ataque.");
        AddES(378, "Botas del Navegante", "Botas sólidas que resisten cualquier tormenta. Anclan a Shell en las corrientes de batalla.");
        AddES(379, "Grebas de las Profundidades", "Botas forjadas en las profundidades del océano. Resistentes a cualquier presión.");
        AddES(380, "Botas Aplastadoras de Huesos", "Botas reforzadas con huesos de monstruos. Cada paso es una amenaza.");
        AddES(381, "Botas de Fortaleza de Magma", "Botas forjadas en el fuego del volcán. Emiten calor que quema a los atacantes.");
        AddES(382, "Botas del Pisoteador de Sangre", "Botas brutales que dejan rastros de destrucción. Los enemigos temen luchar contra ellas.");
        AddES(383, "Botas del Guardián Inquebrantable", "Botas legendarias de un defensor indestructible. Shell se convierte en una fortaleza inmóvil.");
        
        // Vesper Armor (384-411)
        AddES(384, "Sombrero de Bruja de Vesper", "Sombrero simple para tejedores de sueños.");
        AddES(385, "Capucha Extraña de Vesper", "Capucha que susurra secretos.");
        AddES(386, "Capucha Extraña de Vesper", "Capucha pulsando con energía extraña.");
        AddES(387, "Capucha de Energía de Pesadilla de Vesper", "Capucha de energía de pesadilla.");
        AddES(388, "Envoltorios de Desesperación de Vesper", "Envoltorios que se alimentan de desesperación.");
        AddES(389, "Corona de Brujo Oscuro de Vesper", "Corona de magia oscura.");
        AddES(390, "Sombrero de Ferunbras de Vesper", "Sombrero legendario del Maestro de la Oscuridad.");
        AddES(391, "Túnica Roja de Vesper", "Túnica para magos de sueño aspirantes.");
        AddES(392, "Lazos de Almas de Vesper", "Lazos que guían almas.");
        AddES(393, "Túnica de Energía de Vesper", "Túnica crepitando con energía.");
        AddES(394, "Falda de Espíritu de Vesper", "Falda tejida de energía de espíritu.");
        AddES(395, "Capa de Almas de Vesper", "Capa que contiene almas.");
        AddES(396, "Envoltorios de Almas de Vesper", "Envoltorios de almas capturadas.");
        AddES(397, "Túnica de Dragón Arcanum de Vesper", "Túnica legendaria del Dragón Arcanum.");
        AddES(398, "Grebas de Almas de Vesper", "Grebas impregnadas de energía de almas.");
        AddES(399, "Grebas Exóticas de Vesper", "Grebas exóticas de reinos lejanos.");
        AddES(400, "Grebas de Almas de Vesper", "Grebas que guían fuerza de almas.");
        AddES(401, "Pantalones Sangrientos de Vesper", "Pantalones teñidos con magia de sangre.");
        AddES(402, "Grebas de Magma de Vesper", "Grebas forjadas en fuego mágico.");
        AddES(403, "Grebas de Sabiduría de Vesper", "Grebas de sabiduría antigua.");
        AddES(404, "Pantalones de los Antiguos de Vesper", "Pantalones legendarios de los Antiguos.");
        AddES(405, "Zapatillas de Acólito", "Zapatos suaves llevados por nuevos miembros del templo. Llevan oraciones en cada paso.");
        AddES(406, "Zapatos del Templo", "Zapatos tradicionales de los sirvientes de El. Anclan a Vesper en su fe.");
        AddES(407, "Botas de Monje Extraño", "Botas llevadas por monjes que estudian textos prohibidos. Conocimiento y fe se entrelazan.");
        AddES(408, "Envoltorios Bendecidos por Enanos", "Envoltorios encantados por artesanos enanos. La tecnología se encuentra con la divinidad.");
        AddES(409, "Zapatillas de Seda de Vampiro", "Zapatos elegantes tejidos de seda de vampiro. Extraen vida de la tierra misma.");
        AddES(410, "Zapatillas Asesinas de Demonios", "Zapatos verdes bendecidos para destruir el mal. Queman demonios en cada paso.");
        AddES(411, "Botas Expulsoras de Pesadillas", "Botas legendarias que cruzan pesadillas para salvar a los inocentes. Misión definitiva de Vesper.");
        
        // Yubar Armor (412-439)
        AddES(412, "Máscara Tribal de Yubar", "Máscara para guerreros tribales.");
        AddES(413, "Casco Vikingo de Yubar", "Casco para guerreros del norte.");
        AddES(414, "Casco con Cuernos de Yubar", "Casco con cuernos aterradores.");
        AddES(415, "Tocado Ix Resistente de Yubar", "Tocado para la tribu Ix.");
        AddES(416, "Tocado de Miedo de Fuego de Yubar", "Tocado ardiendo con miedo.");
        AddES(417, "Casco Ix Resistente de Yubar", "Casco para guerreros Ix.");
        AddES(418, "Rostro del Apocalipsis de Yubar", "Rostro legendario del Apocalipsis.");
        AddES(419, "Piel de Oso de Yubar", "Armadura de un oso poderoso.");
        AddES(420, "Capa de Piel de Mamut de Yubar", "Capa de un mamut.");
        AddES(421, "Túnica Ix Resistente de Yubar", "Túnica para la tribu Ix.");
        AddES(422, "Manto de Magma de Yubar", "Manto forjado en magma.");
        AddES(423, "Coraza Ix Resistente de Yubar", "Coraza para guerreros Ix.");
        AddES(424, "Armadura de Placas de Lava de Yubar", "Armadura de placas de lava.");
        AddES(425, "Armadura de Gigante de Fuego de Yubar", "Armadura legendaria del Gigante de Fuego.");
        AddES(426, "Grebas Ix Resistentes de Yubar", "Grebas para la tribu Ix.");
        AddES(427, "Pantalones de Piel Mutada de Yubar", "Pantalones hechos de piel mutada.");
        AddES(428, "Falda Ix Resistente de Yubar", "Falda para guerreros Ix.");
        AddES(429, "Grebas Enanas de Yubar", "Grebas enanas sólidas.");
        AddES(430, "Grebas de Aleación de Yubar", "Grebas reforzadas con aleación.");
        AddES(431, "Grebas de Placas de Yubar", "Grebas de placas pesadas.");
        AddES(432, "Grebas Enanas de Yubar", "Grebas legendarias de artesanía enana.");
        AddES(433, "Botas de Combate Temporales", "Botas ensambladas de fragmentos de campo de batalla. Yubar hace con lo que encuentra.");
        AddES(434, "Envoltorios Tribales", "Envoltorios tradicionales del pueblo de Yubar. Lo conectan con sus ancestros.");
        AddES(435, "Grebas de Guerrero Ciervo", "Botas adornadas con astas de ciervo. Otorgan el poder del Rey del Bosque.");
        AddES(436, "Botas del Pisoteador de Colmillos", "Botas brutales con púas en forma de colmillos. Aplastan enemigos bajo los pies.");
        AddES(437, "Botas de Berserker Sangriento", "Botas rojo sangre que encienden la rabia de Yubar. El dolor se convierte en fuerza.");
        AddES(438, "Pisoteador de Almas", "Botas que cosechan las almas de enemigos caídos. Su poder crece con cada batalla.");
        AddES(439, "Botas del Maestro Regreso a Casa", "Botas legendarias que traen espiritualmente a Yubar a casa. Sus ancestros luchan a su lado.");
        
        // Legendary Items (440-443)
        AddES(440, "Casco Alado del Caminante de Sueños", "Casco legendario que otorga a los caminantes de sueños la capacidad de volar. Ataca automáticamente a los enemigos más cercanos.");
        AddES(441, "Armadura de Placas de Magia de Sueño", "Armadura legendaria que protege a los caminantes de sueños de todos los reinos. Apunta automáticamente a los enemigos más cercanos.");
        AddES(442, "Grebas de las Profundidades", "Grebas legendarias forjadas en las partes más profundas de los sueños.");
        AddES(443, "Botas de Caminar sobre Agua", "Botas legendarias que permiten al portador caminar sobre el agua. Tesoro buscado por caminantes de sueños de todos los reinos.");
        
        // Consumables (444-447)
        AddES(444, "Tónico del Soñador", "Un simple remedio elaborado con hierbas del mundo despierto. Incluso el sueño más débil comienza con una gota de esperanza.");
        AddES(445, "Esencia de Ensueño", "Destilada de los recuerdos de sueños pacíficos. Quienes la beben sienten la calidez de sueños olvidados lavando sus heridas.");
        AddES(446, "Vitalidad Lúcida", "Creada por Smoothie usando polvo de estrellas y fragmentos de pesadilla. El líquido brilla con la luz de mil estrellas dormidas.");
        AddES(447, "Elixir de El", "Una bebida sagrada bendecida por la luz de El. La orden de Vesper guarda celosamente su receta, pues puede curar incluso heridas infligidas por las pesadillas más oscuras.");
        
        // Universal Legendary Belt (448)
        AddES(448, "Cinturón de Sueños Infinitos", "Un cinturón legendario que une al portador con el ciclo eterno de los sueños. Su poder crece con cada pesadilla conquistada.");
        
        // Hero-Specific Legendary Belts (449-457)
        AddES(449, "Cinturón Tejedor de Vida de Aurena", "Un cinturón forjado a partir de la investigación robada de Aurena sobre la fuerza vital. Teje la vida y la muerte en perfecto equilibrio.");
        AddES(450, "Faja Cristalina de Bismuth", "Un cinturón de cristal puro que pulsa con la visión de la chica ciega. Ve lo que los ojos no pueden.");
        AddES(451, "Cinturón del Ojo Carmesí de Lacerta", "Un cinturón que canaliza la mirada legendaria de Lacerta. Ningún objetivo escapa a su poder vigilante.");
        AddES(452, "Cinturón de Campeón Duelista de Mist", "El cinturón definitivo de la Casa Astrid. El honor de Mist está tejido en cada hilo.");
        AddES(453, "Cinturón del Árbol del Mundo de Nachia", "Un cinturón cultivado del propio Árbol del Mundo. La furia de la naturaleza fluye a través de él.");
        AddES(454, "Cadena Inquebrantable de Husk", "Un cinturón forjado a partir de una resolución inquebrantable. La determinación de Husk es su fuerza.");
        AddES(455, "Cinturón de Asesino de Sombras de Shell", "Un cinturón de oscuridad perfecta. Los manipuladores de Shell nunca pudieron romper su voluntad.");
        AddES(456, "Cinturón de Inquisidor del Crepúsculo de Vesper", "El cinturón de mayor rango de la Orden de El. Juzga pesadillas y desterrar la oscuridad.");
        AddES(457, "Cinturón de Campeón Ancestral de Yubar", "Un cinturón que invoca a los ancestros de Yubar. Su fuerza fluye a través de cada fibra.");
        
        // Universal Legendary Ring (458)
        AddES(458, "Anillo de Sueños Eternos", "Un anillo legendario que conecta al portador con el reino infinito de los sueños. Su poder trasciende todas las fronteras.");
        
        // Hero-Specific Legendary Rings (459-467)
        AddES(459, "Anillo del Gran Sabio de Aurena", "El anillo del Gran Sabio del Arcanum Lunar. Aurena lo tomó como prueba de su hipocresía.");
        AddES(460, "Anillo Corazón de Gema de Bismuth", "Un anillo de magia cristalina pura. Pulsa con la esencia transformada de la chica ciega.");
        AddES(461, "Anillo del Ejecutor Real de Lacerta", "El anillo del ejecutor de élite de la Guardia Real. Lacerta lo ganó a través de innumerables batallas.");
        AddES(462, "Anillo del Legado Astrid de Mist", "El anillo ancestral de la Casa Astrid. El linaje de Mist fluye a través de su metal.");
        AddES(463, "Anillo del Guardián del Bosque de Nachia", "Un anillo bendecido por los espíritus del bosque de Nachia. El poder de la naturaleza es su esencia.");
        AddES(464, "Anillo de Voluntad Inquebrantable de Husk", "Un anillo forjado a partir de una voluntad inquebrantable. La determinación de Husk es su poder.");
        AddES(465, "Anillo de Sombra Perfecta de Shell", "Un anillo de oscuridad absoluta. Los manipuladores de Shell nunca pudieron controlar su poder.");
        AddES(466, "Anillo de la Llama Sagrada de Vesper", "Un anillo que contiene la esencia pura de la Llama Sagrada de El. Desterrar toda oscuridad.");
        AddES(467, "Anillo de Campeón Tribal de Yubar", "Un anillo que canaliza la fuerza de los ancestros de Yubar. Su furia potencia cada golpe.");
        
        // Universal Legendary Amulet (468)
        AddES(468, "Amuleto de Sueños Infinitos", "Un amuleto legendario que une al portador con el ciclo eterno de los sueños. Su poder trasciende todas las fronteras.");
        
        // Hero-Specific Legendary Amulets (469-477)
        AddES(469, "Amuleto Tejedor de Vida de Aurena", "Un amuleto forjado a partir de la investigación robada de Aurena sobre la fuerza vital. Teje la vida y la muerte en perfecto equilibrio.");
        AddES(470, "Corazón Cristalino de Bismuth", "Un amuleto de cristal puro que pulsa con la visión de la chica ciega. Ve lo que los ojos no pueden.");
        AddES(471, "Amuleto del Ojo Carmesí de Lacerta", "Un amuleto que canaliza la mirada legendaria de Lacerta. Ningún objetivo escapa a su poder vigilante.");
        AddES(472, "Amuleto de Campeón Duelista de Mist", "El amuleto definitivo de la Casa Astrid. El honor de Mist está tejido en cada gema.");
        AddES(473, "Amuleto del Árbol del Mundo de Nachia", "Un amuleto cultivado del propio Árbol del Mundo. La furia de la naturaleza fluye a través de él.");
        AddES(474, "Amuleto Inquebrantable de Husk", "Un amuleto forjado a partir de una resolución inquebrantable. La determinación de Husk es su fuerza.");
        AddES(475, "Amuleto de Asesino de Sombras de Shell", "Un amuleto de oscuridad perfecta. Los manipuladores de Shell nunca pudieron romper su voluntad.");
        AddES(476, "Amuleto de Inquisidor del Crepúsculo de Vesper", "El amuleto de mayor rango de la Orden de El. Juzga pesadillas y desterrar la oscuridad.");
        AddES(477, "Amuleto de Campeón Ancestral de Yubar", "Un amuleto que invoca a los ancestros de Yubar. Su fuerza fluye a través de cada gema.");
    }
    
    private static void AddES(int id, string name, string desc)
    {
        _nameES[id] = name;
        _descES[id] = desc;
    }
    
    // ============================================================
    // PORTUGUESE TRANSLATIONS
    // ============================================================
    private static void InitializePT()
    {
        _namePT = new Dictionary<int, string>();
        _descPT = new Dictionary<int, string>();
        
        // Aurena Weapons (1-18)
        AddPT(1, "Garras do Iniciado", "Garras simples dadas a novos membros da Sociedade Arcanum Lunar. Aurena guardou as suas mesmo após sua expulsão.");
        AddPT(2, "Garras do Erudito", "Garras afiadas em textos proibidos. Os arranhões que deixam sussurram segredos.");
        AddPT(3, "Garras Lunares", "Garras que brilham fracamente sob a luz da lua. Canalizam energia lunar em golpes devastadores.");
        AddPT(4, "Navalhas Arcanum", "Garras inscritas com runas curativas - agora usadas para rasgar ao invés de curar.");
        AddPT(5, "Garras do Exilado", "Forjadas em segredo após seu banimento. Cada corte é um lembrete do que a Sociedade tirou dela.");
        AddPT(6, "Agarre do Herege", "Garras que pertenceram a outro sábio expulso. Seu espírito guia os golpes de Aurena.");
        AddPT(7, "Garras Sacrificiais", "Garras que ficam mais afiadas conforme a força vital de Aurena diminui. A Sociedade considerou esta pesquisa muito perigosa.");
        AddPT(8, "Garras Ligadas pela Geada", "Garras congeladas com gelo do cofre mais profundo da Sociedade. Seu toque entorpece carne e espírito.");
        AddPT(9, "Rasgadores Lunares", "Aprimoradas com encantamentos proibidos. Cada ferida que infligem cura os aliados de Aurena.");
        AddPT(10, "Garras Radiantes", "Garras abençoadas pela luz solar roubada. A pesquisa de Aurena sobre magia de luz enfureceu tanto os cultos do sol quanto da lua.");
        AddPT(11, "Garras de Brasa", "Garras acesas com força vital extraída de sonhadores dispostos. As chamas nunca morrem.");
        AddPT(12, "Garras Umbrales", "Garras mergulhadas na essência de pesadelos. Aurena aprendeu que a escuridão cura o que a luz não pode.");
        AddPT(13, "Ceifador Glacial", "Garras esculpidas em gelo eterno. Congelam o sangue dos inimigos e preservam a força vital de Aurena.");
        AddPT(14, "Garras da Fênix", "Garras forjadas em chamas de fênix e temperadas com o próprio sangue de Aurena. Ardem com raiva inextinguível.");
        AddPT(15, "Rasgador do Vazio", "Garras capazes de rasgar a própria realidade. A Sociedade teme o que Aurena descobrirá a seguir.");
        AddPT(16, "Garras Celestiais", "Garras impregnadas de pura luz estelar. Cada golpe é uma oração pelo conhecimento que os deuses querem esconder.");
        AddPT(17, "Garras do Grande Sábio", "Garras cerimoniais do grande sábio da Sociedade. Aurena as usa como prova de sua hipocrisia.");
        AddPT(18, "Garras do Tecelão da Vida", "Obra-prima de Aurena - garras capazes de roubar força vital sem destruí-la. A Sociedade chama isso de heresia.");
        
        // Bismuth Weapons (19-36)
        AddPT(19, "Grimório Vazio", "Livro de feitiços vazio esperando ser preenchido. Como a própria Bismuth, suas páginas contêm potencial infinito.");
        AddPT(20, "Livro do Aprendiz", "Livro de feitiços para iniciantes. A garota cega que se tornou Bismuth traçou suas páginas com os dedos, sonhando com magia.");
        AddPT(21, "Códice Cristalino", "Livro cujas páginas são feitas de gemas finas. Ressonância com a natureza cristalina de Bismuth.");
        AddPT(22, "Diário do Vagabundo", "Diário de viagem registrando observações da garota cega que nunca viu mas sempre soube.");
        AddPT(23, "Introdução às Gemas", "Guia básico sobre magia cristalina. Suas palavras brilham como o corpo arco-íris de Bismuth.");
        AddPT(24, "Clássico do Cego", "Livro escrito em letras em relevo para cegos. Bismuth o lê por toque e memória.");
        AddPT(25, "Livro das Palavras de Brasa", "Livro cujas páginas queimam eternamente. As chamas falam com Bismuth em cores que ela sente, não vê.");
        AddPT(26, "Dicionário Gelado", "Livro envolto em gelo eterno. Suas páginas geladas preservam conhecimento de antes do início dos sonhos.");
        AddPT(27, "Manuscrito Radiante", "Livro que emite luz interior. Antes de se tornar Bismuth, guiou a garota cega através da escuridão.");
        AddPT(28, "Grimório da Sombra", "Livro que absorve luz. Suas páginas escuras contêm segredos que até Bismuth teme ler.");
        AddPT(29, "Livro de Feitiços Vivo", "Livro consciente que escolheu se fundir com a garota cega. Juntos se tornaram algo maior.");
        AddPT(30, "Códice Prismático", "Livro que decompõe magia em suas cores componentes. Bismuth vê o mundo através da luz que refrata.");
        AddPT(31, "Crônica do Vazio", "Livro registrando as memórias daqueles perdidos na escuridão. Bismuth lê suas histórias para honrá-los.");
        AddPT(32, "Grimório do Coração de Gema", "O livro de feitiços original que se fundiu com a garota cega. Suas páginas pulsam com vida cristalina.");
        AddPT(33, "Livro da Luz Cegante", "Livro tão cegante que pode cegar os videntes. Para Bismuth, é apenas calor.");
        AddPT(34, "Anais Ligados pela Geada", "Livro antigo congelado no tempo. Suas profecias contam de uma gema que caminha como uma garota.");
        AddPT(35, "Livro Sem Nome", "Livro sem nome, como a garota sem visão. Juntos encontraram sua identidade.");
        AddPT(36, "Clássico do Inferno", "Livro ardendo com a paixão de um sonhador que se recusa a deixar a cegueira defini-lo.");
        
        // Lacerta Weapons (37-54)
        AddPT(37, "Rifle do Recruta", "Arma padrão entregue a recrutas da Guarda Real. Lacerta o dominou antes do final da primeira semana de treinamento.");
        AddPT(38, "Rifle Longo de Caçador", "Rifle de caça confiável. Lacerta usou um similar para sustentar sua família antes de se juntar à Guarda.");
        AddPT(39, "Carabina de Patrulha", "Compacta e confiável. Perfeita para longas patrulhas nas fronteiras do reino.");
        AddPT(40, "Rifle do Atirador de Elite", "Rifle equilibrado projetado para aqueles que valorizam precisão sobre poder.");
        AddPT(41, "Mosquete de Pólvora Negra", "Velho mas confiável. O cheiro da pólvora negra lembra Lacerta de sua primeira caça.");
        AddPT(42, "Rifle de Batedor", "Rifle leve para missões de reconhecimento. Velocidade e furtividade superam poder bruto.");
        AddPT(43, "Rifle da Guarda Real", "Arma que Lacerta carregou durante seu serviço. Cada arranhão conta uma história.");
        AddPT(44, "Orgulho do Atirador", "Rifle preciso para aqueles que nunca erram. O olho escarlate de Lacerta vê o que outros não podem.");
        AddPT(45, "Rifle Longo Ardente", "Carregado com balas incendiárias. Inimigos queimam muito tempo após o impacto.");
        AddPT(46, "Carabina Mordida pela Geada", "Dispara balas resfriadas ao zero absoluto. Alvos desaceleram a passo de lesma antes do golpe mortal.");
        AddPT(47, "Rifle Noturno", "Arma para caçar na escuridão. Seus disparos são silenciosos como a sombra.");
        AddPT(48, "Mosquete do Amanhecer", "Abençoado pela luz do amanhecer. Seus disparos perfuram escuridão e engano.");
        AddPT(49, "Olho Escarlate", "Nomeado após o olhar lendário de Lacerta. Nenhum alvo escapa da visão deste rifle.");
        AddPT(50, "Executor Real", "Reservado para a elite da Guarda. Lacerta recebeu este rifle após salvar o reino.");
        AddPT(51, "Canhão Solar", "Rifles que disparam luz concentrada. Cada tiro é um nascer do sol em miniatura.");
        AddPT(52, "Caçador do Vazio", "Rifles que disparam balas de pura escuridão. Alvos desaparecem sem rastro.");
        AddPT(53, "Zero Absoluto", "O ponto mais frio da existência. Até o tempo congela diante dele.");
        AddPT(54, "Bafo do Dragão", "Rifles carregados com balas explosivas incendiárias. Inimigos do reino aprenderam a temer seu rugido.");
        
        // Mist Weapons (55-70)
        AddPT(55, "Florete de Treinamento", "Espada de treinamento da juventude de Mist. Mesmo então, ela superou cada instrutor em Astrid.");
        AddPT(56, "Espada Rapier Nobre", "Espada rapier que combina com a linhagem nobre de Mist. Leve, elegante, mortal.");
        AddPT(57, "Espada Rapier de Duelista", "Espada rapier projetada para combate um contra um. Mist nunca perdeu um duelo.");
        AddPT(58, "Espada Rapier Rápida", "Leve como uma extensão do braço de Mist.");
        AddPT(59, "Espada Rapier de Astrid", "Forjada nas melhores forjas de Astrid. Símbolo da artesanaria real.");
        AddPT(60, "Espada Rapier do Dançarino", "Espada rapier para aqueles que tratam o combate como arte. Os movimentos de Mist são poéticos.");
        AddPT(61, "Espada Rapier Ardente", "Espada rapier cercada de chamas. A raiva de Mist é tão quente quanto sua determinação.");
        AddPT(62, "Espada Rapier da Sombra", "Espada rapier que ataca da escuridão. Inimigos caem antes de ver Mist se mover.");
        AddPT(63, "Espada Rapier Mordida pela Geada", "Espada rapier de aço gelado. Seu toque entorpece corpo e espírito.");
        AddPT(64, "Graça de Camilla", "Presente de alguém que Mist estimava. Ela luta para honrar sua memória.");
        AddPT(65, "Espada Rapier de Parada", "Espada rapier projetada para defesa e ataque. Mist transforma cada ataque em oportunidade.");
        AddPT(66, "Espada Rapier Radiante", "Espada rapier que emite luz interior. Perfura mentiras e sombras.");
        AddPT(67, "Espada Rapier do Inferno", "Espada rapier ardendo com zelo inextinguível. A determinação de Mist não pode ser apagada.");
        AddPT(68, "Espada Rapier da Meia-Noite", "Espada rapier forjada em escuridão absoluta. Ataca com o silêncio da morte.");
        AddPT(69, "Espada Rapier do Inverno", "Espada rapier esculpida em gelo eterno. Seu frio só pode ser igualado pela concentração de Mist em combate.");
        AddPT(70, "Espada Rapier Indomável", "A espada rapier lendária do maior duelista de Astrid. Mist ganhou este título com inúmeras vitórias.");
        
        // Nachia Weapons (71-88)
        AddPT(71, "Cajado de Broto", "Ramo jovem da floresta espiritual. Até brotos respondem ao chamado de Nachia.");
        AddPT(72, "Invocador de Espíritos", "Cajado esculpido para guiar as vozes dos espíritos da floresta. Eles sussurram segredos para Nachia.");
        AddPT(73, "Ramo da Floresta", "Ramo vivo que ainda cresce. A magia da floresta flui através dele.");
        AddPT(74, "Cajado Selvagem", "Selvagem e imprevisível, como a própria Nachia. Os espíritos amam sua energia caótica.");
        AddPT(75, "Cajado do Guardião", "Carregado por aqueles que protegem a floresta. Nachia nasceu com esta responsabilidade.");
        AddPT(76, "Varinha do Mundo Espiritual", "Varinha conectando o mundo material e espiritual. Nachia caminha entre ambos.");
        AddPT(77, "Cajado de Madeira Gelada", "Cajado congelado no inverno eterno. Espíritos frios do norte respondem ao seu chamado.");
        AddPT(78, "Cajado de Raiz de Sombra", "Crescendo nas partes mais profundas da floresta. Espíritos de sombra dançam ao redor.");
        AddPT(79, "Cajado de Madeira de Brasa", "Cajado que nunca para de queimar. Espíritos de fogo são atraídos por seu calor.");
        AddPT(80, "Cajado de Bênção Solar", "Abençoado por espíritos do amanhecer. Sua luz guia almas perdidas para casa.");
        AddPT(81, "Presas de Fenrir", "Cajado com presas de lobo espiritual no topo. O companheiro leal de Nachia guia seu poder.");
        AddPT(82, "Cajado de Carvalho Antigo", "Esculpido de um carvalho milenar. Os espíritos mais antigos lembram quando foi plantado.");
        AddPT(83, "Ramo da Árvore do Mundo", "Ramo da lendária Árvore do Mundo. Todos os espíritos da floresta se curvam diante dele.");
        AddPT(84, "Cajado do Rei dos Espíritos", "O cajado do próprio Rei dos Espíritos. Nachia o ganhou protegendo a floresta.");
        AddPT(85, "Cajado de Geada Eterna", "Cajado de gelo eterno do coração gelado da floresta. Espíritos de inverno obedecem suas ordens.");
        AddPT(86, "Poleiro da Fênix", "Cajado onde espíritos de fogo fazem ninho. Suas chamas trazem renascimento, não destruição.");
        AddPT(87, "Cajado do Bosque Radiante", "Cajado que emite a luz de mil vaga-lumes. Espíritos de esperança dançam dentro.");
        AddPT(88, "Cajado do Guardião do Vazio", "Cajado guardando as fronteiras do mundo. Espíritos de sombra protegem seu portador.");
        
        // Shell Weapons (89-106)
        AddPT(89, "Katana do Noviço", "Katana samurai básico para aqueles que aprendem o caminho da espada. Shell o dominou em poucos dias.");
        AddPT(90, "Katana Silencioso", "Katana samurai projetado para não fazer barulho. Vítimas de Shell nunca ouvem a morte chegando.");
        AddPT(91, "Katana do Assassino", "Estreito e preciso. Shell só precisa de um golpe no lugar certo.");
        AddPT(92, "Katana Assassino", "Eficiente. Prático. Implacável. Como o próprio Shell.");
        AddPT(93, "Katana do Caçador", "Katana samurai para caçar. Shell não para até que o alvo seja eliminado.");
        AddPT(94, "Katana Vazio", "Nomeado após o vazio que Shell sente. Ou não sente.");
        AddPT(95, "Katana do Executor", "Katana samurai que termina inúmeras vidas. Shell não sente nada por nenhuma delas.");
        AddPT(96, "Katana de Precisão", "Katana samurai afinado para explorar fraquezas. Shell calcula o ataque ótimo.");
        AddPT(97, "Katana Mordido pela Geada", "Katana samurai de malícia fria. Seu frio combina com o coração vazio de Shell.");
        AddPT(98, "Katana de Purificação", "Katana samurai brilhante para trabalho sombrio. Shell não sente a ironia.");
        AddPT(99, "Katana da Sombra", "Katana samurai que bebe sombras. Shell se move invisivelmente, ataca silenciosamente.");
        AddPT(100, "Adaga Ardente", "Katana samurai aquecido por fogo interior. Alvos de Shell queimam antes de sangrar.");
        AddPT(101, "Ferramenta do Assassino Perfeito", "A ferramenta definitiva do assassino definitivo. Shell nasceu para esta espada.");
        AddPT(102, "Lâmina Implacável", "Katana samurai que nunca embota, brandido por um perseguidor incansável.");
        AddPT(103, "Assassino do Inferno", "Katana samurai forjado em fogo infernal. Arde com a intensidade do único propósito de Shell.");
        AddPT(104, "Katana do Vazio", "Katana samurai de escuridão absoluta. Como Shell, existe apenas para terminar.");
        AddPT(105, "Katana Congelado", "Katana samurai de gelo eterno. Seu julgamento é tão frio e final quanto Shell.");
        AddPT(106, "Fim Radiante", "A forma definitiva do katana samurai brilhante. Shell executa julgamentos em nome de El.");
        
        // Vesper Weapons (107-124)
        AddPT(107, "Cetro do Noviço", "Cetro simples para novos membros da Ordem da Chama Solar. Vesper começou sua jornada aqui.");
        AddPT(108, "Martelo do Julgamento", "Martelo de guerra usado para executar a vontade de El. Cada golpe é um julgamento sagrado.");
        AddPT(109, "Cajado do Guardião do Fogo", "Arma carregada por aqueles que guardam o fogo sagrado. Seus golpes purificam impureza.");
        AddPT(110, "Escudo do Acólito", "Escudo simples para acólitos da Ordem. A fé é a melhor defesa.");
        AddPT(111, "Escudo do Inquisidor", "Escudo carregado por inquisidores. Viu a queda de inúmeros hereges.");
        AddPT(112, "Defensor do Crepúsculo", "Escudo guardando entre luz e escuridão.");
        AddPT(113, "Martelo do Fanático", "Martelo de guerra ardendo com fanatismo religioso. Inimigos tremem diante dele.");
        AddPT(114, "Martelo Pesado de Purificação", "Martelo de guerra enorme usado para purificar o mal. Um golpe esmaga o pecado.");
        AddPT(115, "Julgamento de El", "Martelo de guerra abençoado pessoalmente por El. Seu julgamento é inapelável.");
        AddPT(116, "Bastião da Fé", "Escudo indestrutível. Enquanto a fé não se extinguir, não quebrará.");
        AddPT(117, "Escudo Sagrado de Chama", "Escudo cercado de fogo sagrado. O mal que o toca é queimado.");
        AddPT(118, "Bastião da Inquisição", "Escudo de um alto inquisidor. Vesper o ganhou com serviço implacável.");
        AddPT(119, "Quebrador do Amanhecer", "Martelo de guerra lendário esmagando a escuridão. Vesper brande a encarnação da raiva de El.");
        AddPT(120, "Destruidor Sagrado", "Martelo de guerra abençoado pelo sumo sacerdote. Seus golpes carregam o peso da raiva sagrada.");
        AddPT(121, "Mão Direita de El", "A arma mais sagrada da Ordem. Vesper é a ferramenta escolhida da vontade de El.");
        AddPT(122, "Fortaleza de Chama", "A defesa definitiva da Ordem. A luz de El protege Vesper de todo dano.");
        AddPT(123, "Vigilância Eterna", "Escudo inquebrantável. Como Vesper, sempre vigia sobre a escuridão.");
        AddPT(124, "Juramento do Paladino do Crepúsculo", "Escudo do líder da Ordem. Vesper jurou vincular sua alma a El.");
        
        // Yubar Weapons (125-142)
        AddPT(125, "Estrela Recém-Nascida", "Estrela que acabou de se formar. Yubar se lembra quando todas as estrelas eram tão pequenas.");
        AddPT(126, "Brasa Cósmica", "Fragmento de fogo estelar. Arde com o calor da própria criação.");
        AddPT(127, "Esfera de Poeira Estelar", "Poeira estelar comprimida esperando ser moldada. Yubar tece galáxias com tal matéria.");
        AddPT(128, "Catalisador de Sonho", "Esfera que amplifica energia de sonhos. Proveniente dos primeiros experimentos de Yubar.");
        AddPT(129, "Núcleo de Nebulosa", "O coração de uma nebulosa distante. Yubar o arrancou da tapeçaria cósmica.");
        AddPT(130, "Semente Celestial", "Semente que se tornará uma estrela. Yubar cultiva inúmeras sementes assim.");
        AddPT(131, "Fragmento de Supernova", "Fragmento de uma estrela explodida. Yubar testemunhou sua morte e preservou seu poder.");
        AddPT(132, "Poço Gravitacional", "Esfera que comprime gravidade. O próprio espaço se curva ao redor.");
        AddPT(133, "Singularidade do Vazio", "Ponto de densidade infinita. Yubar o usa para criar e destruir mundos.");
        AddPT(134, "Coroa Solar", "Fragmento da atmosfera externa do sol. Arde com a raiva da estrela.");
        AddPT(135, "Cometa Congelado", "Núcleo de cometa capturado. Carrega segredos das profundezas do cosmos.");
        AddPT(136, "Forja Estelar", "Miniatura de um núcleo estelar. Yubar a usa para forjar nova realidade.");
        AddPT(137, "Relíquia do Big Bang", "Fragmento do Big Bang da criação. Contém a origem do universo.");
        AddPT(138, "Tear Cósmico", "Ferramenta que tece realidade. Yubar a usa para remodelar o destino.");
        AddPT(139, "Criação Radiante", "A luz da própria criação. Yubar a usou para dar à luz as primeiras estrelas.");
        AddPT(140, "Coração da Entropia", "A essência da decomposição cósmica. Tudo termina, e Yubar decide quando.");
        AddPT(141, "Zero Absoluto", "O ponto mais frio da existência. Até o tempo congela diante dele.");
        AddPT(142, "Núcleo de Catástrofe", "O coração da destruição cósmica. Yubar só o libera em momentos críticos.");
        
        // Rings (143-162)
        AddPT(143, "Anel do Sonhador", "Anel simples carregado por aqueles que acabaram de entrar no mundo dos sonhos. Emite esperança fraca.");
        AddPT(144, "Anel de Poeira Estelar", "Forjado de poeira estelar condensada. Sonhadores dizem que brilha mais forte em perigo.");
        AddPT(145, "Marca do Sono", "Gravura de um sono pacífico. Portadores se recuperam mais rápido de feridas.");
        AddPT(146, "Anel de Espírito", "Anel que captura espíritos errantes. Sua luz guia caminhos através de sonhos sombrios.");
        AddPT(147, "Anel do Vagabundo", "Carregado por vagabundos dos reinos dos sonhos. Viu inúmeros caminhos esquecidos.");
        AddPT(148, "Anel de Fragmento de Pesadelo", "Anel contendo fragmentos de pesadelos derrotados. Sua escuridão fortalece os bravos.");
        AddPT(149, "Guardião da Memória", "Anel que preserva memórias preciosas. Smoothie vende similares em sua loja.");
        AddPT(150, "Anel do Arcanum Lunar", "Anel da Sociedade Arcanum Lunar. Aurena carregou um similar antes de seu exílio.");
        AddPT(151, "Marca da Floresta Espiritual", "Anel abençoado pelos espíritos da floresta de Nachia. A força da natureza flui através.");
        AddPT(152, "Insígnia da Guarda Real", "Anel que Lacerta carregou durante seu serviço. Ainda carrega o peso da responsabilidade.");
        AddPT(153, "Glória do Duelista", "Anel passado entre duelistas nobres. A família de Mist aprecia tais anéis.");
        AddPT(154, "Amuleto de Brasa de Chama", "Faísca selada do fogo sagrado. A Ordem de Vesper guarda essas relíquias.");
        AddPT(155, "Fragmento de Livro de Feitiços", "Anel feito de magia cristalina de Bismuth. Zumbido com energia arcanum.");
        AddPT(156, "Marca do Caçador", "Anel que rastreia presa. Controladores de Shell os usam para monitorar suas ferramentas.");
        AddPT(157, "Bênção de El", "Anel abençoado pelo próprio El. Sua luz dissipa pesadelos e cura os feridos.");
        AddPT(158, "Marca do Senhor dos Pesadelos", "Tomado de um senhor de pesadelos derrotado. Seu poder sombrio corrompe e fortalece.");
        AddPT(159, "Legado de Astrid", "Anel da casa nobre de Astrid. Ancestrais de Mist o carregaram em inúmeros duelos.");
        AddPT(160, "Olho do Vidente", "Anel capaz de ver através do véu dos sonhos. Passado, presente e futuro se misturam.");
        AddPT(161, "Anel do Sonho Primordial", "Forjado no amanhecer dos sonhos. Contém a essência do primeiro sonhador.");
        AddPT(162, "Sono Eterno", "Anel que toca o sono mais profundo. Portadores não temem nem morte nem despertar.");
        
        // Amulets (163-183)
        AddPT(163, "Pingente do Sonhador", "Pingente simples carregado por aqueles que acabaram de entrar no mundo dos sonhos. Emite esperança fraca.");
        AddPT(164, "Pingente do Toque de Fogo", "Pingente tocado pelo fogo. Seu calor dissipa frio e medo.");
        AddPT(165, "Lágrima Escarlate", "Rubi em forma de lágrima de sangue. Seu poder vem do sacrifício.");
        AddPT(166, "Amuleto de Âmbar", "Âmbar contendo uma criatura antiga. Suas memórias fortalecem o portador.");
        AddPT(167, "Medalha do Crepúsculo", "Medalha piscando entre luz e escuridão. Novos membros de Vesper carregam estas.");
        AddPT(168, "Colar de Poeira Estelar", "Colar polvilhado com estrelas cristalinas. Smoothie as coleta de sonhos caídos.");
        AddPT(169, "Olho de Esmeralda", "Gema verde capaz de ver através de ilusões. Pesadelos não podem escapar de seu olhar.");
        AddPT(170, "Coração do Espírito da Floresta", "Gema contendo a bênção dos espíritos da floresta. Guardiões de Nachia apreciam estas.");
        AddPT(171, "Marca do Arcanum Lunar", "Marca de arquivos proibidos. Aurena arrisca tudo estudando-as.");
        AddPT(172, "Cristal de Bismuth", "Fragmento de magia pura cristalina. Ressonância com energia arcanum.");
        AddPT(173, "Amuleto de Brasa de Chama", "Faísca selada do fogo sagrado. A Ordem de Vesper guarda essas relíquias.");
        AddPT(174, "Pingente de Presa de Pesadelo", "Presa de um pesadelo poderoso, agora um troféu. Shell carrega uma como lembrança.");
        AddPT(175, "Medalha da Guarda Real", "Medalha de honra da Guarda Real. Lacerta ganhou muitas antes de seu exílio.");
        AddPT(176, "Brasão de Astrid", "Brasão nobre da família de Astrid. O legado da família de Mist pende desta corrente.");
        AddPT(177, "Lágrima Sagrada de El", "Lágrima derramada pelo próprio El. Sua luz protege contra os pesadelos mais sombrios.");
        AddPT(178, "Esmeralda Primordial", "Gema da primeira floresta. Contém os sonhos de espíritos antigos.");
        AddPT(179, "Olho do Senhor dos Pesadelos", "Olho de um senhor de pesadelos derrotado. Vê todos os medos e os explora.");
        AddPT(180, "Coração da Chama", "O núcleo do próprio fogo sagrado. Apenas os mais piedosos podem carregá-lo.");
        AddPT(181, "Oráculo do Vidente", "Amuleto capaz de ver todos os futuros possíveis. A realidade se curva diante de suas profecias.");
        AddPT(182, "Bússola do Viajante do Vazio", "Amuleto apontando para o vazio. Aqueles que o seguem nunca retornam da mesma maneira.");
        AddPT(183, "Amuleto de Midas", "Amuleto lendário tocado pelo sonho do ouro. Inimigos derrotados deixam cair ouro adicional.");
        AddPT(478, "Coletor de Poeira Estelar", "Amuleto lendário que coleta poeira estelar cristalizada de inimigos derrotados. Cada eliminação produz essência de sonho preciosa (base 1-5, escala com nível de melhoria e força do monstro).");
        
        // Belts (184-203)
        AddPT(184, "Cinto do Vagabundo", "Cinto de tecido simples carregado por aqueles que vagam entre sonhos. Ancora a alma.");
        AddPT(185, "Corda do Sonhador", "Corda tecida de fios de sonho. Pulsa suavemente a cada batida do coração.");
        AddPT(186, "Cinto de Poeira Estelar", "Cinto adornado com poeira estelar cristalina. Smoothie vende acessórios similares.");
        AddPT(187, "Cinto da Sentinela", "Cinto sólido carregado por guardiões dos sonhos. Nunca afrouxa, nunca falha.");
        AddPT(188, "Cinto Místico", "Cinto impregnado de magia fraca. Pica quando pesadelos se aproximam.");
        AddPT(189, "Cinto do Caçador", "Cinto de couro com bolsas de suprimentos. Essencial para longas viagens em sonhos.");
        AddPT(190, "Cinto do Peregrino", "Carregado por aqueles que buscam a luz de El. Oferece conforto nos sonhos mais sombrios.");
        AddPT(191, "Cinto do Caçador de Pesadelos", "Cinto feito de pesadelos derrotados. Sua essência fortalece o portador.");
        AddPT(192, "Cinto Arcanum", "Cinto da Sociedade Arcanum Lunar. Amplifica energia mágica.");
        AddPT(193, "Cinto de Chama", "Cinto abençoado pela Ordem de Vesper. Arde com fogo de proteção.");
        AddPT(194, "Cinto de Espada do Duelista", "Cinto elegante carregado por guerreiros nobres de Astrid. Carrega espadas e honra.");
        AddPT(195, "Corda do Tecelão de Espíritos", "Cinto tecido por espíritos da floresta para Nachia. Zumbido com energia natural.");
        AddPT(196, "Cinto de Ferramentas do Assassino", "Cinto com compartimentos ocultos. Controladores de Shell os usam para equipar suas ferramentas.");
        AddPT(197, "Cinto de Coldre de Revólver", "Cinto projetado para as armas de fogo de Lacerta. Puxada rápida, morte mais rápida.");
        AddPT(198, "Cinto Sagrado de El", "Cinto abençoado pelo próprio El. Sua luz ancora a alma contra toda escuridão.");
        AddPT(199, "Corrente do Senhor dos Pesadelos", "Cinto forjado das correntes de um senhor de pesadelos. Vincula medo à vontade do portador.");
        AddPT(200, "Cinto do Sonho Primordial", "Cinto do primeiro sonho. Contém o eco da própria criação.");
        AddPT(201, "Cinto de Raiz da Árvore do Mundo", "Cinto que cresce das raízes da Árvore do Mundo. A floresta de Nachia abençoa sua criação.");
        AddPT(202, "Legado de Astrid", "Cinto familiar da família de Astrid. O sangue de Mist está tecido em seu tecido.");
        AddPT(203, "Cinto do Inquisidor do Crepúsculo", "Cinto do mais alto nível de Vesper. Julga todos que se param diante dele.");
        
        // Armor Sets - Bone Sentinel (204-215)
        AddPT(204, "Máscara da Sentinela de Osso", "Máscara esculpida do crânio de um pesadelo caído. Seus olhos vazios veem através de ilusões.");
        AddPT(205, "Elmo do Guardião do Mundo Espiritual", "Elmo impregnado de espíritos da floresta de Nachia. Sussurros de proteção cercam o portador.");
        AddPT(206, "Coroa do Senhor dos Pesadelos", "Coroa forjada de pesadelos conquistados. O portador governa sobre o próprio medo.");
        AddPT(207, "Colete da Sentinela de Osso", "Armadura das costelas de criaturas que morreram em sonhos. Sua proteção dura além da morte.");
        AddPT(208, "Couraça do Guardião do Mundo Espiritual", "Couraça tecida de sonhos cristalizados. Move e se adapta a golpes que chegam.");
        AddPT(209, "Couraça do Senhor dos Pesadelos", "Armadura pulsando com energia sombria. Pesadelos se curvam diante de seu portador.");
        AddPT(210, "Perneiras da Sentinela de Osso", "Perneiras reforçadas com ossos de pesadelo. Nunca se cansam, nunca cedem.");
        AddPT(211, "Perneiras do Guardião do Mundo Espiritual", "Perneiras abençoadas por espíritos da floresta. O portador se move como vento através de folhas.");
        AddPT(212, "Perneiras do Senhor dos Pesadelos", "Perneiras que bebem sombras. A escuridão fortalece cada passo.");
        AddPT(213, "Botas da Sentinela de Osso", "Botas para caminhar silenciosamente nos reinos dos sonhos. Os mortos não fazem barulho.");
        AddPT(214, "Botas de Ferro do Guardião do Mundo Espiritual", "Botas que não deixam rastro nos reinos dos sonhos. Perfeitas para caçadores de pesadelos.");
        AddPT(215, "Botas do Senhor dos Pesadelos", "Botas que cruzam mundos. A realidade se curva sob cada passo.");
        
        // Aurena Armor (216-243)
        AddPT(216, "Chapéu de Aprendiz de Aurena", "Chapéu simples carregado por noviços do Arcanum Lunar.");
        AddPT(217, "Turbante Místico de Aurena", "Turbante impregnado de energia lunar fraca.");
        AddPT(218, "Coroa Encantadora de Aurena", "Coroa que melhora a afinidade mágica do portador.");
        AddPT(219, "Coroa Antiga de Aurena", "Relíquia antiga transmitida através de gerações.");
        AddPT(220, "Bandana de Falcão de Aurena", "Bandana abençoada por espíritos do céu.");
        AddPT(221, "Coroa de Poder de Aurena", "Coroa poderosa que emite poder arcanum.");
        AddPT(222, "Coroa Lunar de Aurena", "Coroa lendária de um mestre do Arcanum Lunar.");
        AddPT(223, "Túnica Azul de Aurena", "Túnica simples carregada por magos lunares.");
        AddPT(224, "Túnica de Mago de Aurena", "Túnica apreciada por magos praticantes.");
        AddPT(225, "Túnica Tecida com Feitiços de Aurena", "Túnica tecida com fios mágicos.");
        AddPT(226, "Túnica Iluminadora de Aurena", "Túnica que guia a sabedoria lunar.");
        AddPT(227, "Sudário de Sonho de Aurena", "Sudário tecido de sonhos cristalizados.");
        AddPT(228, "Túnica Real de Escamas de Aurena", "Túnica adequada para a família real lunar.");
        AddPT(229, "Túnica da Rainha de Gelo de Aurena", "Túnica lendária da própria Rainha de Gelo.");
        AddPT(230, "Perneiras Azuis de Aurena", "Perneiras simples carregadas por magos aspirantes.");
        AddPT(231, "Saia de Fibra de Aurena", "Saia leve que permite movimento livre.");
        AddPT(232, "Perneiras Élficas de Aurena", "Perneiras elegantes projetadas por elfos.");
        AddPT(233, "Perneiras Iluminadoras de Aurena", "Perneiras abençoadas pela sabedoria.");
        AddPT(234, "Saia Glacial de Aurena", "Saia impregnada de magia de geada.");
        AddPT(235, "Calças de Geada de Aurena", "Calças que emitem força de frio.");
        AddPT(236, "Perneiras Magníficas de Aurena", "Perneiras lendárias de elegância incomparável.");
        AddPT(237, "Chinelos Suaves de Lua", "Sapatos suaves carregados por acólitos do Arcanum Lunar. Amortecem cada passo com luz lunar.");
        AddPT(238, "Sandálias do Peregrino", "Sandálias simples abençoadas pelos anciãos da Sociedade. Aurena ainda se lembra de seu calor.");
        AddPT(239, "Sapatos de Mística Oriental", "Sapatos exóticos de terras distantes. Guiam energia curativa através da terra.");
        AddPT(240, "Graça do Caminhante de Sonhos", "Botas que caminham entre o mundo desperto e o mundo dos sonhos. Perfeitas para curandeiros procurando almas perdidas.");
        AddPT(241, "Botas do Caminhante de Almas", "Botas impregnadas da essência de curandeiros falecidos. Sua sabedoria guia cada passo.");
        AddPT(242, "Botas do Rastreador de Almas", "Botas escuras que rastreiam os feridos. Aurena as usa para encontrar aqueles que precisam de ajuda... ou aqueles que a machucaram.");
        AddPT(243, "Asas do Sábio Caído", "Botas lendárias que supostamente concedem a capacidade de voar a corações puros. O exílio de Aurena provou sua qualificação.");
        
        // Bismuth Armor (244-271)
        AddPT(244, "Chapéu Cônico de Bismuth", "Chapéu simples para aprendizes cristalinos.");
        AddPT(245, "Chapéu de Esmeralda de Bismuth", "Chapéu incrustado com cristais de esmeralda.");
        AddPT(246, "Máscara Glacial de Bismuth", "Máscara feita de cristais gelados.");
        AddPT(247, "Máscara Alarharim de Bismuth", "Máscara antiga de poder cristalino.");
        AddPT(248, "Elmo Prismático de Bismuth", "Elmo que refrata luz em arco-íris.");
        AddPT(249, "Coroa Refletora de Bismuth", "Coroa que reflete energia mágica.");
        AddPT(250, "Coroa Incandescente de Bismuth", "Coroa lendária ardendo com fogo cristalino.");
        AddPT(251, "Túnica de Gelo de Bismuth", "Túnica resfriada por magia cristalina.");
        AddPT(252, "Túnica de Monge de Bismuth", "Túnica simples para meditação.");
        AddPT(253, "Túnica Glacial de Bismuth", "Túnica congelada em geada eterna.");
        AddPT(254, "Armadura Cristalina de Bismuth", "Armadura formada de cristais vivos.");
        AddPT(255, "Armadura Prismática de Bismuth", "Armadura que curva a própria luz.");
        AddPT(256, "Armadura de Placas Congelada de Bismuth", "Armadura de placas de geada eterna.");
        AddPT(257, "Armadura de Placas Sagrada de Bismuth", "Armadura lendária abençoada por deuses cristalinos.");
        AddPT(258, "Perneiras de Cota de Malha de Bismuth", "Perneiras básicas com elos cristalinos.");
        AddPT(259, "Perneiras de Fibra de Bismuth", "Perneiras leves para magos cristalinos.");
        AddPT(260, "Perneiras de Esmeralda de Bismuth", "Perneiras adornadas com cristais de esmeralda.");
        AddPT(261, "Calças Estranhas de Bismuth", "Calças pulsando com energia estranha.");
        AddPT(262, "Perneiras Prismáticas de Bismuth", "Perneiras brilhando com luz prismática.");
        AddPT(263, "Perneiras Alarharim de Bismuth", "Perneiras antigas de Alarharim.");
        AddPT(264, "Perneiras de Falcão de Bismuth", "Perneiras lendárias da Ordem do Falcão.");
        AddPT(265, "Chinelos do Erudito", "Sapatos confortáveis para trabalho longo em bibliotecas. Conhecimento flui através de suas solas.");
        AddPT(266, "Envoltórios Alarharim", "Envoltórios antigos de uma civilização perdida. Sussurram feitiços esquecidos.");
        AddPT(267, "Sapatos de Gelo", "Botas esculpidas de gelo eterno. Bismuth deixa padrões de geada onde caminha.");
        AddPT(268, "Passo de Flor de Geada", "Botas elegantes florescendo com cristais de gelo. Cada passo cria um jardim de geada.");
        AddPT(269, "Botas de Ressonância Cristalina", "Botas que amplificam energia mágica. Os cristais zumbem com poder não explorado.");
        AddPT(270, "Botas Arcanum Prismáticas", "Botas que refratam luz em energia mágica pura. A realidade se curva ao redor de cada passo.");
        AddPT(271, "Botas do Caminhante do Vazio", "Botas lendárias que cruzam o próprio vazio. Bismuth vê caminhos que outros não podem imaginar.");
        
        // Lacerta Armor (272-299)
        AddPT(272, "Elmo de Couro de Lagarto", "Elmo básico para guerreiros dragão.");
        AddPT(273, "Elmo de Cota de Malha de Lagarto", "Elmo sólido de cota de malha.");
        AddPT(274, "Elmo de Guerreiro de Lagarto", "Elmo carregado por guerreiros experientes.");
        AddPT(275, "Elmo de Escamas de Dragão de Lagarto", "Elmo forjado de escamas de dragão.");
        AddPT(276, "Elmo Real de Lagarto", "Elmo adequado para a família real dragão.");
        AddPT(277, "Elmo de Elite Homem-Dragão de Lagarto", "Elmo de elite da Ordem Homem-Dragão.");
        AddPT(278, "Elmo de Ouro de Lagarto", "Elmo de ouro lendário do Rei Dragão.");
        AddPT(279, "Armadura de Couro de Lagarto", "Armadura básica para guerreiros dragão.");
        AddPT(280, "Armadura de Escamas de Lagarto", "Armadura reforçada com escamas.");
        AddPT(281, "Armadura de Cavaleiro de Lagarto", "Armadura pesada para cavaleiros dragão.");
        AddPT(282, "Cota de Malha de Escamas de Dragão de Lagarto", "Cota de malha forjada de escamas de dragão.");
        AddPT(283, "Cota de Malha Real Homem-Dragão de Lagarto", "Cota de malha real da Ordem Homem-Dragão.");
        AddPT(284, "Armadura de Placas de Falcão de Lagarto", "Armadura de placas da Ordem do Falcão.");
        AddPT(285, "Armadura de Ouro de Lagarto", "Armadura de ouro lendária do Rei Dragão.");
        AddPT(286, "Perneiras de Couro de Lagarto", "Perneiras básicas para guerreiros dragão.");
        AddPT(287, "Perneiras Rebitadas de Lagarto", "Perneiras reforçadas com rebites.");
        AddPT(288, "Perneiras de Cavaleiro de Lagarto", "Perneiras pesadas para cavaleiros dragão.");
        AddPT(289, "Perneiras de Escamas de Dragão de Lagarto", "Perneiras forjadas de escamas de dragão.");
        AddPT(290, "Perneiras Reais de Lagarto", "Perneiras adequadas para a família real.");
        AddPT(291, "Perneiras Magníficas de Lagarto", "Perneiras magníficas de artesanaria de mestre.");
        AddPT(292, "Perneiras de Ouro de Lagarto", "Perneiras de ouro lendárias do Rei Dragão.");
        AddPT(293, "Botas de Couro Desgastadas", "Botas simples que viram muitas batalhas. Carregam o aroma da aventura.");
        AddPT(294, "Botas de Combate Remendadas", "Botas reparadas inúmeras vezes. Cada remendo conta uma história de sobrevivência.");
        AddPT(295, "Botas do Guerreiro de Aço", "Botas pesadas forjadas para combate. Ancoram Lacerta ao chão durante a batalha.");
        AddPT(296, "Perneiras do Guardião", "Botas carregadas por guardiões de elite. Nunca recuam, nunca se rendem.");
        AddPT(297, "Perneiras de Escamas de Dragão", "Botas feitas de escamas de dragão. Concedem ao portador a resistência do dragão.");
        AddPT(298, "Botas do Senhor da Guerra Homem-Dragão", "Botas de antigos generais homem-dragão. Temíveis em qualquer campo de batalha.");
        AddPT(299, "Botas do Mestre de Ouro", "Botas lendárias do maior guerreiro de uma era. O destino de Lacerta espera.");
        
        // Mist Armor (300-327)
        AddPT(300, "Turbante de Mist", "Turbante simples que esconde a identidade.");
        AddPT(301, "Turbante Leve de Mist", "Turbante leve e respirável.");
        AddPT(302, "Turbante de Visão Noturna de Mist", "Turbante que melhora a visão noturna.");
        AddPT(303, "Bandana de Relâmpago de Mist", "Bandana carregada com energia elétrica.");
        AddPT(304, "Capuz de Cobra de Mist", "Capuz mortal como uma serpente.");
        AddPT(305, "Máscara do Rastreador do Inferno de Mist", "Máscara que rastreia presa.");
        AddPT(306, "Sussurrador Sombrio de Mist", "Máscara lendária que sussurra morte.");
        AddPT(307, "Arnês de Couro de Mist", "Arnês leve para agilidade.");
        AddPT(308, "Capa de Guarda-Florestal de Mist", "Capa para movimento rápido.");
        AddPT(309, "Túnica da Meia-Noite de Mist", "Túnica para golpes noturnos.");
        AddPT(310, "Túnica de Relâmpago de Mist", "Túnica carregada com relâmpagos.");
        AddPT(311, "Armadura de Tensão de Mist", "Armadura pulsando com energia elétrica.");
        AddPT(312, "Armadura de Mestre Arqueiro de Mist", "Armadura de um mestre arqueiro.");
        AddPT(313, "Capa do Senhor Sombrio de Mist", "Capa lendária do Senhor Sombrio.");
        AddPT(314, "Perneiras de Guarda-Florestal de Mist", "Perneiras carregadas por guarda-florestais.");
        AddPT(315, "Perneiras de Sobrevivência de Selva de Mist", "Perneiras de sobrevivência de selva.");
        AddPT(316, "Sarongue da Meia-Noite de Mist", "Sarongue para operações noturnas.");
        AddPT(317, "Perneiras de Relâmpago de Mist", "Perneiras carregadas com relâmpagos.");
        AddPT(318, "Perneiras de Gafanhoto de Mist", "Perneiras para saltos surpreendentes.");
        AddPT(319, "Perneiras de Demônio Verde de Mist", "Perneiras de velocidade demoníaca.");
        AddPT(320, "Perneiras do Caminhante de Almas de Mist", "Perneiras lendárias que cruzam mundos.");
        AddPT(321, "Botas de Batedor Temporárias", "Botas montadas de fragmentos. Leves e silenciosas, perfeitas para reconhecimento.");
        AddPT(322, "Botas do Corredor de Pântano", "Botas impermeáveis para terreno difícil. Mist conhece cada atalho.");
        AddPT(323, "Caminhante Rápido", "Botas encantadas que aceleram os passos do portador. Perfeitas para tática de golpear e correr.");
        AddPT(324, "Botas do Caçador de Cervo", "Botas que rastreiam presa.");
        AddPT(325, "Botas de Golpe de Relâmpago", "Botas carregadas com energia de relâmpago.");
        AddPT(326, "Botas de Combate do Caminhante de Fogo", "Botas capazes de caminhar através do fogo.");
        AddPT(327, "Pisoteador de Sofrimento", "Botas lendárias que pisoteiam todo sofrimento.");
        
        // Nachia Armor (328-355)
        AddPT(328, "Chapéu de Pele de Nachia", "Chapéu simples para caçadores da floresta.");
        AddPT(329, "Tocado de Penas de Nachia", "Tocado adornado com penas.");
        AddPT(330, "Máscara de Xamã de Nachia", "Máscara de poder espiritual.");
        AddPT(331, "Capuz de Terra de Nachia", "Capuz abençoado por espíritos da terra.");
        AddPT(332, "Elmo de Natureza de Nachia", "Elmo impregnado de força natural.");
        AddPT(333, "Coroa de Árvore de Nachia", "Coroa que cresce de uma árvore antiga.");
        AddPT(334, "Coroa de Folha de Nachia", "Coroa lendária do Guardião da Floresta.");
        AddPT(335, "Armadura de Pele de Nachia", "Armadura feita de criaturas da floresta.");
        AddPT(336, "Armadura Indígena de Nachia", "Armadura tradicional das tribos da floresta.");
        AddPT(337, "Manto de Madeira Verde de Nachia", "Manto tecido de videiras vivas.");
        AddPT(338, "Capa de Terra de Nachia", "Capa abençoada por espíritos da terra.");
        AddPT(339, "Abraço de Natureza de Nachia", "Armadura unida com a natureza.");
        AddPT(340, "Armadura de Ninho de Pântano de Nachia", "Armadura de pântanos profundos.");
        AddPT(341, "Túnica de Folha de Nachia", "Túnica lendária do Guardião da Floresta.");
        AddPT(342, "Shorts de Pele de Mamute de Nachia", "Shorts quentes feitos de pele de mamute.");
        AddPT(343, "Shorts de Couro de Nachia", "Perneiras de caça tradicionais.");
        AddPT(344, "Perneiras de Cervo de Nachia", "Perneiras feitas de pele de cervo.");
        AddPT(345, "Perneiras de Terra de Nachia", "Perneiras abençoadas por espíritos da terra.");
        AddPT(346, "Perneiras de Folha de Nachia", "Perneiras tecidas de folhas mágicas.");
        AddPT(347, "Tanga de Homem-Javali de Nachia", "Tanga de um poderoso homem-javali.");
        AddPT(348, "Perneiras de Caça Sangrenta", "Perneiras lendárias de caça sangrenta.");
        AddPT(349, "Botas Forradas de Pele", "Botas quentes e confortáveis.");
        AddPT(350, "Botas de Pele de Texugo", "Botas feitas de pele de texugo.");
        AddPT(351, "Botas de Golpe de Cobra", "Botas rápidas como uma serpente.");
        AddPT(352, "Envoltórios do Rastejador da Floresta", "Envoltórios silenciosos na floresta.");
        AddPT(353, "Botas do Caçador de Terra", "Botas que rastreiam presa.");
        AddPT(354, "Botas de Guarda-Florestal de Flor de Febre", "Botas adornadas com flores de febre. Concedem velocidade febril e precisão mortal.");
        AddPT(355, "Botas do Predador Sangrento", "Botas lendárias encharcadas do sangue de inúmeras caçadas. Nachia se tornou o predador supremo.");
        
        // Shell Armor (356-383)
        AddPT(356, "Elmo Danificado de Shell", "Elmo danificado do mundo subterrâneo.");
        AddPT(357, "Máscara Quebrada de Shell", "Máscara quebrada que ainda oferece proteção.");
        AddPT(358, "Elmo do Senhor de Ossos de Shell", "Elmo forjado dos restos do Senhor de Ossos.");
        AddPT(359, "Elmo de Esqueleto de Shell", "Elmo formado de ossos.");
        AddPT(360, "Elmo de Morte de Shell", "Elmo da própria morte.");
        AddPT(361, "Guardião de Esqueleto Nokferatu de Shell", "Elmo de ossos de vampiro.");
        AddPT(362, "Elmo Demoníaco de Shell", "Elmo lendário do Senhor Demônio.");
        AddPT(363, "Sudário Funerário de Shell", "Sudário da tumba.");
        AddPT(364, "Capa Antiga de Shell", "Capa antiga de tempos antigos.");
        AddPT(365, "Capa de Ossos Nokferatu de Shell", "Capa tecida de ossos.");
        AddPT(366, "Capa de Almas de Shell", "Capa de almas inquietas.");
        AddPT(367, "Túnica de Morte de Shell", "Túnica da morte.");
        AddPT(368, "Túnica do Mundo Subterrâneo de Shell", "Túnica das profundezas.");
        AddPT(369, "Armadura Demoníaca de Shell", "Armadura lendária do Senhor Demônio.");
        AddPT(370, "Avental Danificado de Shell", "Avental danificado mas útil.");
        AddPT(371, "Avental de Ossos Mutados de Shell", "Avental feito de ossos mutados.");
        AddPT(372, "Envoltórios de Espinhos Nokferatu de Shell", "Envoltórios com espinhos.");
        AddPT(373, "Guardião de Carne Nokferatu de Shell", "Proteção feita de carne preservada.");
        AddPT(374, "Perneiras Sangrentas de Shell", "Perneiras encharcadas de sangue.");
        AddPT(375, "Caminhante de Sangue Nokferatu de Shell", "Perneiras que caminham em sangue.");
        AddPT(376, "Perneiras Demoníacas de Shell", "Perneiras lendárias do Senhor Demônio.");
        AddPT(377, "Perneiras Blindadas", "Botas metálicas pesadas que esmagam tudo sob os pés. A defesa é o melhor ataque.");
        AddPT(378, "Botas do Navegante", "Botas sólidas que resistem a qualquer tempestade. Ancoram Shell nas correntes de batalha.");
        AddPT(379, "Perneiras das Profundezas", "Botas forjadas nas profundezas do oceano. Resistentes a qualquer pressão.");
        AddPT(380, "Botas Esmagadoras de Ossos", "Botas reforçadas com ossos de monstros. Cada passo é uma ameaça.");
        AddPT(381, "Botas de Fortaleza de Magma", "Botas forjadas no fogo do vulcão. Emitem calor que queima os atacantes.");
        AddPT(382, "Botas do Pisoteador de Sangue", "Botas brutais que deixam rastros de destruição. Inimigos temem lutar contra elas.");
        AddPT(383, "Botas do Guardião Inquebrantável", "Botas lendárias de um defensor indestrutível. Shell se torna uma fortaleza imóvel.");
        
        // Vesper Armor (384-411)
        AddPT(384, "Chapéu de Bruxa de Vesper", "Chapéu simples para tecelões de sonhos.");
        AddPT(385, "Capuz Estranho de Vesper", "Capuz que sussurra segredos.");
        AddPT(386, "Capuz Estranho de Vesper", "Capuz pulsando com energia estranha.");
        AddPT(387, "Capuz de Energia de Pesadelo de Vesper", "Capuz de energia de pesadelo.");
        AddPT(388, "Envoltórios de Desespero de Vesper", "Envoltórios que se alimentam de desespero.");
        AddPT(389, "Coroa de Bruxo Sombrio de Vesper", "Coroa de magia sombria.");
        AddPT(390, "Chapéu de Ferunbras de Vesper", "Chapéu lendário do Mestre das Trevas.");
        AddPT(391, "Túnica Vermelha de Vesper", "Túnica para magos de sonho aspirantes.");
        AddPT(392, "Laços de Almas de Vesper", "Laços que guiam almas.");
        AddPT(393, "Túnica de Energia de Vesper", "Túnica crepitando com energia.");
        AddPT(394, "Saia de Espírito de Vesper", "Saia tecida de energia de espírito.");
        AddPT(395, "Capa de Almas de Vesper", "Capa que contém almas.");
        AddPT(396, "Envoltórios de Almas de Vesper", "Envoltórios de almas capturadas.");
        AddPT(397, "Túnica de Dragão Arcanum de Vesper", "Túnica lendária do Dragão Arcanum.");
        AddPT(398, "Perneiras de Almas de Vesper", "Perneiras impregnadas de energia de almas.");
        AddPT(399, "Perneiras Exóticas de Vesper", "Perneiras exóticas de reinos distantes.");
        AddPT(400, "Perneiras de Almas de Vesper", "Perneiras que guiam força de almas.");
        AddPT(401, "Calças Sangrentas de Vesper", "Calças tingidas com magia de sangue.");
        AddPT(402, "Perneiras de Magma de Vesper", "Perneiras forjadas em fogo mágico.");
        AddPT(403, "Perneiras de Sabedoria de Vesper", "Perneiras de sabedoria antiga.");
        AddPT(404, "Calças dos Antigos de Vesper", "Calças lendárias dos Antigos.");
        AddPT(405, "Chinelos de Acólito", "Sapatos suaves carregados por novos membros do templo. Carregam orações em cada passo.");
        AddPT(406, "Sapatos do Templo", "Sapatos tradicionais dos servos de El. Ancoram Vesper em sua fé.");
        AddPT(407, "Botas de Monge Estranho", "Botas carregadas por monges que estudam textos proibidos. Conhecimento e fé se entrelaçam.");
        AddPT(408, "Envoltórios Abençoados por Anões", "Envoltórios encantados por artesãos anões. Tecnologia encontra divindade.");
        AddPT(409, "Chinelos de Seda de Vampiro", "Sapatos elegantes tecidos de seda de vampiro. Extraem vida da própria terra.");
        AddPT(410, "Chinelos Assassinos de Demônios", "Sapatos verdes abençoados para destruir o mal. Queimam demônios em cada passo.");
        AddPT(411, "Botas Expulsoras de Pesadelos", "Botas lendárias que cruzam pesadelos para salvar os inocentes. Missão definitiva de Vesper.");
        
        // Yubar Armor (412-439)
        AddPT(412, "Máscara Tribal de Yubar", "Máscara para guerreiros tribais.");
        AddPT(413, "Elmo Viking de Yubar", "Elmo para guerreiros do norte.");
        AddPT(414, "Elmo com Chifres de Yubar", "Elmo com chifres aterrorizantes.");
        AddPT(415, "Tocado Ix Resistente de Yubar", "Tocado para a tribo Ix.");
        AddPT(416, "Tocado de Medo de Fogo de Yubar", "Tocado ardendo com medo.");
        AddPT(417, "Elmo Ix Resistente de Yubar", "Elmo para guerreiros Ix.");
        AddPT(418, "Rosto do Apocalipse de Yubar", "Rosto lendário do Apocalipse.");
        AddPT(419, "Pele de Urso de Yubar", "Armadura de um urso poderoso.");
        AddPT(420, "Capa de Pele de Mamute de Yubar", "Capa de um mamute.");
        AddPT(421, "Túnica Ix Resistente de Yubar", "Túnica para a tribo Ix.");
        AddPT(422, "Manto de Magma de Yubar", "Manto forjado em magma.");
        AddPT(423, "Couraça Ix Resistente de Yubar", "Couraça para guerreiros Ix.");
        AddPT(424, "Armadura de Placas de Lava de Yubar", "Armadura de placas de lava.");
        AddPT(425, "Armadura de Gigante de Fogo de Yubar", "Armadura lendária do Gigante de Fogo.");
        AddPT(426, "Perneiras Ix Resistentes de Yubar", "Perneiras para a tribo Ix.");
        AddPT(427, "Calças de Pele Mutada de Yubar", "Calças feitas de pele mutada.");
        AddPT(428, "Saia Ix Resistente de Yubar", "Saia para guerreiros Ix.");
        AddPT(429, "Perneiras Anãs de Yubar", "Perneiras anãs sólidas.");
        AddPT(430, "Perneiras de Liga de Yubar", "Perneiras reforçadas com liga.");
        AddPT(431, "Perneiras de Placas de Yubar", "Perneiras de placas pesadas.");
        AddPT(432, "Perneiras Anãs de Yubar", "Perneiras lendárias de artesanaria anã.");
        AddPT(433, "Botas de Combate Temporárias", "Botas montadas de fragmentos de campo de batalha. Yubar faz com o que encontra.");
        AddPT(434, "Envoltórios Tribais", "Envoltórios tradicionais do povo de Yubar. O conectam com seus ancestrais.");
        AddPT(435, "Perneiras de Guerreiro Cervo", "Botas adornadas com chifres de cervo. Concedem o poder do Rei da Floresta.");
        AddPT(436, "Botas do Pisoteador de Presas", "Botas brutais com pontas em forma de presas. Esmagam inimigos sob os pés.");
        AddPT(437, "Botas de Berserker Sangrento", "Botas vermelho-sangue que acendem a raiva de Yubar. Dor se torna força.");
        AddPT(438, "Pisoteador de Almas", "Botas que colhem as almas de inimigos caídos. Seu poder cresce com cada batalha.");
        AddPT(439, "Botas do Mestre Retorno para Casa", "Botas lendárias que trazem espiritualmente Yubar para casa. Seus ancestrais lutam ao seu lado.");
        
        // Legendary Items (440-443)
        AddPT(440, "Elmo Alado do Caminhante de Sonhos", "Elmo lendário que concede aos caminhantes de sonhos a capacidade de voar. Ataca automaticamente os inimigos mais próximos.");
        AddPT(441, "Armadura de Placas de Magia de Sonho", "Armadura lendária que protege caminhantes de sonhos de todos os reinos. Mira automaticamente nos inimigos mais próximos.");
        AddPT(442, "Perneiras das Profundezas", "Perneiras lendárias forjadas nas partes mais profundas dos sonhos.");
        AddPT(443, "Botas de Caminhar sobre Água", "Botas lendárias que permitem ao portador caminhar sobre a água. Tesouro procurado por caminhantes de sonhos de todos os reinos.");
        
        // Consumables (444-447)
        AddPT(444, "Tônico do Sonhador", "Um remédio simples feito com ervas do mundo desperto. Até o sonho mais fraco começa com uma gota de esperança.");
        AddPT(445, "Essência de Devaneio", "Destilada das memórias de sonos tranquilos. Quem a bebe sente o calor de sonhos esquecidos lavando suas feridas.");
        AddPT(446, "Vitalidade Lúcida", "Criada por Smoothie usando poeira estelar e fragmentos de pesadelo. O líquido brilha com a luz de mil estrelas adormecidas.");
        AddPT(447, "Elixir de El", "Uma bebida sagrada abençoada pela luz de El. A ordem de Vesper guarda zelosamente sua receita, pois pode curar até feridas causadas pelos pesadelos mais sombrios.");
        
        // Universal Legendary Belt (448)
        AddPT(448, "Cinto dos Sonhos Infinitos", "Um cinto lendário que une o portador ao ciclo eterno dos sonhos. Seu poder cresce com cada pesadelo conquistado.");
        
        // Hero-Specific Legendary Belts (449-457)
        AddPT(449, "Cinto Tecelão de Vida de Aurena", "Um cinto forjado a partir da pesquisa roubada de Aurena sobre força vital. Tecer vida e morte em equilíbrio perfeito.");
        AddPT(450, "Cinturão Cristalino de Bismuth", "Um cinto de cristal puro que pulsa com a visão da garota cega. Vê o que os olhos não podem.");
        AddPT(451, "Cinto do Olho Carmesim de Lacerta", "Um cinto que canaliza o olhar lendário de Lacerta. Nenhum alvo escapa de seu poder vigilante.");
        AddPT(452, "Cinto de Campeão Duelista de Mist", "O cinto definitivo da Casa Astrid. A honra de Mist está tecida em cada fio.");
        AddPT(453, "Cinto da Árvore do Mundo de Nachia", "Um cinto cultivado da própria Árvore do Mundo. A fúria da natureza flui através dele.");
        AddPT(454, "Corrente Inquebrantável de Husk", "Um cinto forjado a partir de uma resolução inquebrantável. A determinação de Husk é sua força.");
        AddPT(455, "Cinto de Assassino das Sombras de Shell", "Um cinto de escuridão perfeita. Os manipuladores de Shell nunca conseguiram quebrar sua vontade.");
        AddPT(456, "Cinto de Inquisidor do Crepúsculo de Vesper", "O cinto de maior patente da Ordem de El. Julga pesadelos e banir a escuridão.");
        AddPT(457, "Cinto de Campeão Ancestral de Yubar", "Um cinto que invoca os ancestrais de Yubar. Sua força flui através de cada fibra.");
        
        // Universal Legendary Ring (458)
        AddPT(458, "Anel dos Sonhos Eternos", "Um anel lendário que conecta o portador ao reino infinito dos sonhos. Seu poder transcende todas as fronteiras.");
        
        // Hero-Specific Legendary Rings (459-467)
        AddPT(459, "Anel do Grande Sábio de Aurena", "O anel do Grande Sábio do Arcanum Lunar. Aurena o tomou como prova de sua hipocrisia.");
        AddPT(460, "Anel Coração de Gema de Bismuth", "Um anel de magia cristalina pura. Pulso com a essência transformada da garota cega.");
        AddPT(461, "Anel do Executor Real de Lacerta", "O anel do executor de elite da Guarda Real. Lacerta o ganhou através de inúmeras batalhas.");
        AddPT(462, "Anel do Legado Astrid de Mist", "O anel ancestral da Casa Astrid. A linhagem de Mist flui através de seu metal.");
        AddPT(463, "Anel do Guardião da Floresta de Nachia", "Um anel abençoado pelos espíritos da floresta de Nachia. O poder da natureza é sua essência.");
        AddPT(464, "Anel de Vontade Inquebrantável de Husk", "Um anel forjado a partir de uma vontade inquebrantável. A determinação de Husk é seu poder.");
        AddPT(465, "Anel de Sombra Perfeita de Shell", "Um anel de escuridão absoluta. Os manipuladores de Shell nunca conseguiram controlar seu poder.");
        AddPT(466, "Anel da Chama Sagrada de Vesper", "Um anel contendo a essência pura da Chama Sagrada de El. Banir toda escuridão.");
        AddPT(467, "Anel de Campeão Tribal de Yubar", "Um anel que canaliza a força dos ancestrais de Yubar. Sua fúria potencializa cada golpe.");
        
        // Universal Legendary Amulet (468)
        AddPT(468, "Amuleto dos Sonhos Infinitos", "Um amuleto lendário que une o portador ao ciclo eterno dos sonhos. Seu poder transcende todas as fronteiras.");
        
        // Hero-Specific Legendary Amulets (469-477)
        AddPT(469, "Amuleto Tecelão de Vida de Aurena", "Um amuleto forjado a partir da pesquisa roubada de Aurena sobre força vital. Tecer vida e morte em equilíbrio perfeito.");
        AddPT(470, "Coração Cristalino de Bismuth", "Um amuleto de cristal puro que pulsa com a visão da garota cega. Vê o que os olhos não podem.");
        AddPT(471, "Amuleto do Olho Carmesim de Lacerta", "Um amuleto que canaliza o olhar lendário de Lacerta. Nenhum alvo escapa de seu poder vigilante.");
        AddPT(472, "Amuleto de Campeão Duelista de Mist", "O amuleto definitivo da Casa Astrid. A honra de Mist está tecida em cada gema.");
        AddPT(473, "Amuleto da Árvore do Mundo de Nachia", "Um amuleto cultivado da própria Árvore do Mundo. A fúria da natureza flui através dele.");
        AddPT(474, "Amuleto Inquebrantável de Husk", "Um amuleto forjado a partir de uma resolução inquebrantável. A determinação de Husk é sua força.");
        AddPT(475, "Amuleto de Assassino das Sombras de Shell", "Um amuleto de escuridão perfeita. Os manipuladores de Shell nunca conseguiram quebrar sua vontade.");
        AddPT(476, "Amuleto de Inquisidor do Crepúsculo de Vesper", "O amuleto de maior patente da Ordem de El. Julga pesadelos e banir a escuridão.");
        AddPT(477, "Amuleto de Campeão Ancestral de Yubar", "Um amuleto que invoca os ancestrais de Yubar. Sua força flui através de cada gema.");
    }
    
    private static void AddPT(int id, string name, string desc)
    {
        _namePT[id] = name;
        _descPT[id] = desc;
    }
    
    // ============================================================
    // RUSSIAN TRANSLATIONS
    // ============================================================
    private static void InitializeRU()
    {
        _nameRU = new Dictionary<int, string>();
        _descRU = new Dictionary<int, string>();
        
        // Aurena Weapons (1-18)
        AddRU(1, "Когти Посвящённого", "Простые когти, данные новым членам Лунного Общества Арканум. Аурена сохранила свои даже после изгнания.");
        AddRU(2, "Когти Учёного", "Когти, заточенные на запрещённых текстах. Оставляемые ими царапины шепчут секреты.");
        AddRU(3, "Лунные Когти", "Когти, слабо светящиеся при лунном свете. Они направляют лунную энергию в разрушительные удары.");
        AddRU(4, "Бритвы Арканум", "Когти с выгравированными рунами исцеления - теперь используются для разрывания, а не для лечения.");
        AddRU(5, "Когти Изгнанника", "Выкованные втайне после её изгнания. Каждый удар напоминает о том, что Общество отняло у неё.");
        AddRU(6, "Хватка Еретика", "Когти, некогда принадлежавшие другому изгнанному мудрецу. Их дух направляет удары Аурены.");
        AddRU(7, "Жертвенные Когти", "Когти, становящиеся острее по мере угасания жизненной силы Аурены. Общество сочло это исследование слишком опасным.");
        AddRU(8, "Когти, Скованные Морозом", "Когти, замороженные льдом из самого глубокого хранилища Общества. Их прикосновение оцепеняет плоть и дух.");
        AddRU(9, "Лунные Разрыватели", "Усиленные запретными заклинаниями. Каждая нанесённая ими рана исцеляет союзников Аурены.");
        AddRU(10, "Сияющие Когти", "Когти, благословлённые украденным солнечным светом. Исследования Аурены в области магии света разгневали как культы солнца, так и луны.");
        AddRU(11, "Когти Тлеющих Углей", "Когти, зажжённые жизненной силой, извлечённой из согласных сновидцев. Пламя никогда не гаснет.");
        AddRU(12, "Теневые Когти", "Когти, погружённые в сущность кошмаров. Аурена узнала, что тьма исцеляет то, что свет не может.");
        AddRU(13, "Ледниковый Жнец", "Когти, вырезанные из вечного льда. Они замораживают кровь врагов и сохраняют жизненную силу Аурены.");
        AddRU(14, "Когти Феникса", "Когти, выкованные в пламени феникса и закалённые собственной кровью Аурены. Они горят неугасимым гневом.");
        AddRU(15, "Разрыватель Пустоты", "Когти, способные разорвать саму реальность. Общество боится того, что Аурена откроет дальше.");
        AddRU(16, "Небесные Когти", "Когти, пропитанные чистым звёздным светом. Каждый удар - молитва о знании, которое боги хотят скрыть.");
        AddRU(17, "Когти Великого Мудреца", "Церемониальные когти великого мудреца Общества. Аурена использует их как доказательство их лицемерия.");
        AddRU(18, "Когти Ткача Жизни", "Шедевр Аурены - когти, способные украсть жизненную силу, не уничтожая её. Общество называет это ересью.");
        
        // Bismuth Weapons (19-36)
        AddRU(19, "Пустой Гримуар", "Пустая книга заклинаний, ожидающая заполнения. Как сама Бисмут, её страницы содержат бесконечный потенциал.");
        AddRU(20, "Книга Ученика", "Книга заклинаний для начинающих. Слепая девочка, ставшая Бисмут, проводила пальцами по её страницам, мечтая о магии.");
        AddRU(21, "Кристальный Кодекс", "Книга, страницы которой сделаны из тонких самоцветов. Резонирует с кристальной природой Бисмут.");
        AddRU(22, "Дневник Странника", "Дневник путешествий, записывающий наблюдения слепой девочки, которая никогда не видела, но всегда знала.");
        AddRU(23, "Введение в Самоцветы", "Базовое руководство по кристальной магии. Её слова сверкают, как радужное тело Бисмут.");
        AddRU(24, "Классика Слепого", "Книга, написанная рельефными буквами для слепых. Бисмут читает её на ощупь и по памяти.");
        AddRU(25, "Книга Слов Тлеющих Углей", "Книга, страницы которой вечно горят. Пламя говорит с Бисмут цветами, которые она чувствует, а не видит.");
        AddRU(26, "Ледяной Словарь", "Книга, обёрнутая вечным льдом. Её ледяные страницы сохраняют знания из времени до начала снов.");
        AddRU(27, "Сияющая Рукопись", "Книга, излучающая внутренний свет. До того, как стать Бисмут, она вела слепую девочку через тьму.");
        AddRU(28, "Гримуар Тени", "Книга, поглощающая свет. Её тёмные страницы содержат секреты, которые даже Бисмут боится читать.");
        AddRU(29, "Живая Книга Заклинаний", "Сознательная книга, выбравшая слияние со слепой девочкой. Вместе они стали чем-то большим.");
        AddRU(30, "Призматический Кодекс", "Книга, разлагающая магию на составляющие цвета. Бисмут видит мир через свет, который она преломляет.");
        AddRU(31, "Хроника Пустоты", "Книга, записывающая воспоминания тех, кто потерялся во тьме. Бисмут читает их истории, чтобы почтить их.");
        AddRU(32, "Гримуар Сердца Самоцвета", "Оригинальная книга заклинаний, слившаяся со слепой девочкой. Её страницы пульсируют кристальной жизнью.");
        AddRU(33, "Книга Ослепляющего Света", "Книга настолько ослепляющая, что может ослепить зрячих. Для Бисмут это просто тепло.");
        AddRU(34, "Летопись, Скованная Морозом", "Древняя книга, замороженная во времени. Её пророчества рассказывают о самоцвете, который ходит как девочка.");
        AddRU(35, "Книга Без Имени", "Книга без имени, как девочка без зрения. Вместе они нашли свою идентичность.");
        AddRU(36, "Классика Ада", "Книга, горящая страстью сновидца, который отказывается позволить слепоте определять его.");
        
        // Lacerta Weapons (37-54)
        AddRU(37, "Винтовка Рекрута", "Стандартное оружие, выданное рекрутам Королевской Гвардии. Ласерта овладел им до конца первой недели тренировок.");
        AddRU(38, "Длинная Охотничья Винтовка", "Надёжная охотничья винтовка. Ласерта использовал подобную, чтобы содержать семью до присоединения к Гвардии.");
        AddRU(39, "Патрульный Карабин", "Компактный и надёжный. Идеален для длинных патрулей на границах королевства.");
        AddRU(40, "Винтовка Снайпера", "Сбалансированная винтовка, разработанная для тех, кто ценит точность превыше силы.");
        AddRU(41, "Чёрный Пороховой Мушкет", "Старый, но надёжный. Запах чёрного пороха напоминает Ласерте о его первой охоте.");
        AddRU(42, "Разведывательная Винтовка", "Лёгкая винтовка для разведывательных миссий. Скорость и скрытность превосходят грубую силу.");
        AddRU(43, "Винтовка Королевской Гвардии", "Оружие, которое Ласерта носил во время службы. Каждая царапина рассказывает историю.");
        AddRU(44, "Гордость Снайпера", "Точная винтовка для тех, кто никогда не промахивается. Алый глаз Ласерты видит то, чего другие не могут.");
        AddRU(45, "Длинная Пылающая Винтовка", "Заряжена зажигательными пулями. Враги горят долго после попадания.");
        AddRU(46, "Карабин Морозный Укус", "Стреляет пулями, охлаждёнными до абсолютного нуля. Цели замедляются до скорости улитки перед смертельным ударом.");
        AddRU(47, "Ночная Винтовка", "Оружие для охоты в темноте. Её выстрелы тихи, как тень.");
        AddRU(48, "Мушкет Рассвета", "Благословлён светом рассвета. Её выстрелы пронзают тьму и обман.");
        AddRU(49, "Алый Глаз", "Названа в честь легендарного взгляда Ласерты. Ни одна цель не ускользает от взгляда этой винтовки.");
        AddRU(50, "Королевский Палач", "Зарезервирована для элиты Гвардии. Ласерта получил эту винтовку после спасения королевства.");
        AddRU(51, "Солнечная Пушка", "Винтовки, стреляющие концентрированным светом. Каждый выстрел - миниатюрный восход солнца.");
        AddRU(52, "Охотник Пустоты", "Винтовки, стреляющие чисто тёмными пулями. Цели исчезают без следа.");
        AddRU(53, "Абсолютный Ноль", "Самая холодная точка существования. Даже время замерзает перед ним.");
        AddRU(54, "Драконий Дых", "Винтовки, заряжённые взрывными зажигательными пулями. Враги королевства научились бояться его рёва.");
        
        // Mist Weapons (55-70)
        AddRU(55, "Тренировочная Рапира", "Тренировочный меч из юности Мист. Даже тогда она превзошла каждого инструктора в Астриде.");
        AddRU(56, "Благородная Рапира", "Рапира, соответствующая благородному происхождению Мист. Лёгкая, элегантная, смертоносная.");
        AddRU(57, "Рапира Дуэлянта", "Рапира, разработанная для боя один на один. Мист никогда не проигрывала дуэль.");
        AddRU(58, "Быстрая Рапира", "Лёгкая, как продолжение руки Мист.");
        AddRU(59, "Рапира Астрида", "Выкована в лучших кузницах Астрида. Символ королевского мастерства.");
        AddRU(60, "Рапира Танцора", "Рапира для тех, кто относится к бою как к искусству. Движения Мист поэтичны.");
        AddRU(61, "Пылающая Рапира", "Рапира, окружённая пламенем. Гнев Мист так же горяч, как её решимость.");
        AddRU(62, "Рапира Тени", "Рапира, бьющая из темноты. Враги падают, прежде чем увидят движение Мист.");
        AddRU(63, "Рапира Морозный Укус", "Рапира из замёрзшей стали. Её прикосновение оцепеняет тело и дух.");
        AddRU(64, "Милость Камиллы", "Подарок от кого-то, кого Мист ценила. Она сражается, чтобы почтить их память.");
        AddRU(65, "Рапира Парирования", "Рапира, разработанная для защиты и атаки. Мист превращает каждую атаку в возможность.");
        AddRU(66, "Сияющая Рапира", "Рапира, излучающая внутренний свет. Она пронзает ложь и тени.");
        AddRU(67, "Рапира Ада", "Рапира, горящая неугасимым рвением. Решимость Мист нельзя погасить.");
        AddRU(68, "Рапира Полуночи", "Рапира, выкованная в абсолютной темноте. Бьёт с тишиной смерти.");
        AddRU(69, "Зимняя Рапира", "Рапира, вырезанная из вечного льда. Её холод может сравниться только с концентрацией Мист в бою.");
        AddRU(70, "Неукротимая Рапира", "Легендарная рапира величайшего дуэлянта Астрида. Мист завоевала этот титул бесчисленными победами.");
        
        // Nachia Weapons (71-88)
        AddRU(71, "Посох Ростка", "Молодая ветвь из духовного леса. Даже ростки отвечают на зов Начии.");
        AddRU(72, "Призыватель Духов", "Посох, вырезанный для направления голосов духов леса. Они шепчут секреты Начии.");
        AddRU(73, "Ветвь Леса", "Живая ветвь, которая всё ещё растёт. Магия леса течёт через неё.");
        AddRU(74, "Дикий Посох", "Дикий и непредсказуемый, как сама Начия. Духи любят её хаотичную энергию.");
        AddRU(75, "Посох Стража", "Носится теми, кто защищает лес. Начия родилась с этой ответственностью.");
        AddRU(76, "Жезл Духовного Мира", "Жезл, соединяющий материальный и духовный миры. Начия ходит между ними.");
        AddRU(77, "Посох Морозного Дерева", "Посох, замёрзший в вечной зиме. Холодные духи севера отвечают на его зов.");
        AddRU(78, "Посох Теневого Корня", "Растущий в самых глубоких частях леса. Теневые духи танцуют вокруг.");
        AddRU(79, "Посох Дерева Тлеющих Углей", "Посох, который никогда не перестаёт гореть. Духи огня притягиваются его теплом.");
        AddRU(80, "Посох Солнечного Благословения", "Благословлён духами рассвета. Его свет ведёт потерянные души домой.");
        AddRU(81, "Клыки Фенрира", "Посох с клыками духовного волка на вершине. Верный спутник Начии направляет его силу.");
        AddRU(82, "Посох Старого Дуб", "Вырезан из тысячелетнего дуба. Самые старые духи помнят, когда он был посажен.");
        AddRU(83, "Ветвь Древа Мира", "Ветвь легендарного Древа Мира. Все духи леса склоняются перед ним.");
        AddRU(84, "Посох Короля Духов", "Посох самого Короля Духов. Начия завоевала его, защищая лес.");
        AddRU(85, "Посох Вечного Мороза", "Посох из вечного льда замороженного сердца леса. Зимние духи подчиняются его приказам.");
        AddRU(86, "Насест Феникса", "Посох, на котором гнездятся духи огня. Его пламя приносит возрождение, а не разрушение.");
        AddRU(87, "Посох Сияющей Рощи", "Посох, излучающий свет тысячи светлячков. Духи надежды танцуют внутри.");
        AddRU(88, "Посох Стража Пустоты", "Посох, охраняющий границы мира. Теневые духи защищают его носителя.");
        
        // Shell Weapons (89-106)
        AddRU(89, "Катана Новичка", "Базовый катана самурая для тех, кто изучает путь меча. Шелл овладел им за несколько дней.");
        AddRU(90, "Тихий Катана", "Катана самурая, разработанный, чтобы не издавать звука. Жертвы Шелла никогда не слышат приближения смерти.");
        AddRU(91, "Катана Убийцы", "Узкий и точный. Шелл нужен только один удар в правильном месте.");
        AddRU(92, "Катана Убийца", "Эффективный. Практичный. Безжалостный. Как сам Шелл.");
        AddRU(93, "Катана Охотника", "Катана самурая для охоты. Шелл не останавливается, пока цель не устранена.");
        AddRU(94, "Пустой Катана", "Назван в честь пустоты, которую чувствует Шелл. Или не чувствует.");
        AddRU(95, "Катана Палача", "Катана самурая, завершающий бесчисленные жизни. Шелл ничего не чувствует ни к одной из них.");
        AddRU(96, "Катана Точности", "Катана самурая, настроенный на использование слабостей. Шелл вычисляет оптимальную атаку.");
        AddRU(97, "Катана Морозный Укус", "Катана самурая холодной злобы. Его холод соответствует пустому сердцу Шелла.");
        AddRU(98, "Катана Очищения", "Яркий катана самурая для тёмной работы. Шелл не чувствует иронии.");
        AddRU(99, "Катана Тени", "Катана самурая, пьющий тени. Шелл движется невидимо, бьёт бесшумно.");
        AddRU(100, "Пылающий Кинжал", "Катана самурая, нагретый внутренним огнём. Цели Шелла горят, прежде чем истекают кровью.");
        AddRU(101, "Инструмент Идеального Убийцы", "Окончательный инструмент окончательного убийцы. Шелл родился для этого меча.");
        AddRU(102, "Безжалостный Клинок", "Катана самурая, который никогда не тупится, размахиваемый неутомимым преследователем.");
        AddRU(103, "Убийца Ада", "Катана самурая, выкованный в адском огне. Горит с интенсивностью единственной цели Шелла.");
        AddRU(104, "Катана Пустоты", "Катана самурая абсолютной тьмы. Как Шелл, существует только для завершения.");
        AddRU(105, "Замёрзший Катана", "Катана самурая из вечного льда. Его приговор так же холоден и окончателен, как Шелл.");
        AddRU(106, "Сияющий Конец", "Окончательная форма яркого катана самурая. Шелл исполняет приговоры во имя Эль.");
        
        // Vesper Weapons (107-124)
        AddRU(107, "Скипетр Новичка", "Простой скипетр для новых членов Ордена Солнечного Пламени. Веспер начала своё путешествие здесь.");
        AddRU(108, "Молот Суда", "Боевой молот, используемый для исполнения воли Эль. Каждый удар - священный приговор.");
        AddRU(109, "Посох Стража Огня", "Оружие, носимое теми, кто охраняет священный огонь. Его удары очищают нечистоту.");
        AddRU(110, "Щит Аколита", "Простой щит для аколитов Ордена. Вера - лучшая защита.");
        AddRU(111, "Щит Инквизитора", "Щит, носимым инквизиторами. Видел падение бесчисленных еретиков.");
        AddRU(112, "Защитник Сумерек", "Щит, охраняющий между светом и тьмой.");
        AddRU(113, "Молот Фанатика", "Боевой молот, горящий религиозным фанатизмом. Враги дрожат перед ним.");
        AddRU(114, "Тяжёлый Молот Очищения", "Огромный боевой молот, используемый для очищения зла. Один удар сокрушает грех.");
        AddRU(115, "Суд Эль", "Боевой молот, лично благословлённый Эль. Его приговор не подлежит обжалованию.");
        AddRU(116, "Бастион Веры", "Неразрушимый щит. Пока вера не угаснет, он не сломается.");
        AddRU(117, "Священный Щит Пламени", "Щит, окружённый священным огнём. Зло, касающееся его, сжигается.");
        AddRU(118, "Бастион Инквизиции", "Щит высокого инквизитора. Веспер завоевала его безжалостной службой.");
        AddRU(119, "Сокрушитель Рассвета", "Легендарный боевой молот, сокрушающий тьму. Веспер размахивает воплощением гнева Эль.");
        AddRU(120, "Священный Разрушитель", "Боевой молот, благословлённый верховным жрецом. Его удары несут вес священного гнева.");
        AddRU(121, "Правая Рука Эль", "Самое священное оружие Ордена. Веспер - избранный инструмент воли Эль.");
        AddRU(122, "Крепость Пламени", "Окончательная защита Ордена. Свет Эль защищает Веспер от всякого вреда.");
        AddRU(123, "Вечная Бдительность", "Непоколебимый щит. Как Веспер, всегда бдит над тьмой.");
        AddRU(124, "Клятва Паладина Сумерек", "Щит лидера Ордена. Веспер поклялась связать свою душу с Эль.");
        
        // Yubar Weapons (125-142)
        AddRU(125, "Новорождённая Звезда", "Звезда, которая только что сформировалась. Юбар помнит, когда все звёзды были такими маленькими.");
        AddRU(126, "Космические Тлеющие Угли", "Фрагмент звёздного огня. Горит с жаром самого творения.");
        AddRU(127, "Сфера Звёздной Пыли", "Сжатая звёздная пыль, ожидающая формовки. Юбар ткёт галактики из такой материи.");
        AddRU(128, "Катализатор Сна", "Сфера, усиливающая энергию снов. Происходит из первых экспериментов Юбара.");
        AddRU(129, "Ядро Туманности", "Сердце далёкой туманности. Юбар вырвал его из космического гобелена.");
        AddRU(130, "Небесное Семя", "Семя, которое станет звездой. Юбар выращивает бесчисленные такие семена.");
        AddRU(131, "Фрагмент Сверхновой", "Фрагмент взорвавшейся звезды. Юбар стал свидетелем её смерти и сохранил её силу.");
        AddRU(132, "Гравитационный Колодец", "Сфера, сжимающая гравитацию. Само пространство изгибается вокруг.");
        AddRU(133, "Сингулярность Пустоты", "Точка бесконечной плотности. Юбар использует её для создания и уничтожения миров.");
        AddRU(134, "Солнечная Корона", "Фрагмент внешней атмосферы солнца. Горит с гневом звезды.");
        AddRU(135, "Замёрзшая Комета", "Захваченное ядро кометы. Несёт секреты из глубин космоса.");
        AddRU(136, "Звёздная Кузница", "Миниатюра звёздного ядра. Юбар использует её для ковки новой реальности.");
        AddRU(137, "Реликвия Большого Взрыва", "Фрагмент Большого Взрыва творения. Содержит происхождение вселенной.");
        AddRU(138, "Космический Ткацкий Станок", "Инструмент, ткущий реальность. Юбар использует его для переделки судьбы.");
        AddRU(139, "Сияющее Творение", "Свет самого творения. Юбар использовал его, чтобы породить первые звёзды.");
        AddRU(140, "Сердце Энтропии", "Сущность космического распада. Всё заканчивается, и Юбар решает когда.");
        AddRU(141, "Абсолютный Ноль", "Самая холодная точка существования. Даже время замерзает перед ним.");
        AddRU(142, "Ядро Катастрофы", "Сердце космического разрушения. Юбар освобождает его только в критические моменты.");
        
        // Rings (143-162)
        AddRU(143, "Кольцо Сновидца", "Простое кольцо, носимое теми, кто только что вошёл в мир снов. Излучает слабую надежду.");
        AddRU(144, "Кольцо Звёздной Пыли", "Выковано из конденсированной звёздной пыли. Сновидцы говорят, что оно светится ярче в опасности.");
        AddRU(145, "Метка Сна", "Гравюра мирного сна. Носители быстрее восстанавливаются от ран.");
        AddRU(146, "Кольцо Духа", "Кольцо, захватывающее блуждающих духов. Их свет направляет пути сквозь тёмные сны.");
        AddRU(147, "Кольцо Странника", "Носится странниками царств снов. Видело бесчисленные забытые пути.");
        AddRU(148, "Кольцо Фрагмента Кошмара", "Кольцо, содержащее фрагменты побеждённых кошмаров. Его тьма укрепляет храбрых.");
        AddRU(149, "Страж Памяти", "Кольцо, сохраняющее драгоценные воспоминания. Смузи продаёт подобные в своём магазине.");
        AddRU(150, "Кольцо Лунного Арканум", "Кольцо из Лунного Общества Арканум. Аурена носила подобное до своего изгнания.");
        AddRU(151, "Метка Духовного Леса", "Кольцо, благословлённое духами леса Начии. Сила природы течёт через него.");
        AddRU(152, "Знак Королевской Гвардии", "Кольцо, которое Ласерта носил во время службы. Всё ещё несёт вес ответственности.");
        AddRU(153, "Слава Дуэлянта", "Кольцо, передаваемое между благородными дуэлянтами. Семья Мист ценит такие кольца.");
        AddRU(154, "Амулет Тлеющих Углей Пламени", "Запечатанная искра священного огня. Орден Веспер охраняет эти реликвии.");
        AddRU(155, "Фрагмент Книги Заклинаний", "Кольцо, сделанное из кристальной магии Бисмут. Жужжит энергией арканум.");
        AddRU(156, "Метка Охотника", "Кольцо, отслеживающее добычу. Контролёры Шелла используют их для мониторинга своих инструментов.");
        AddRU(157, "Благословение Эль", "Кольцо, благословлённое самим Эль. Его свет рассеивает кошмары и исцеляет раненых.");
        AddRU(158, "Метка Повелителя Кошмаров", "Взято у побеждённого повелителя кошмаров. Его тёмная сила развращает и укрепляет.");
        AddRU(159, "Наследие Астрида", "Кольцо из благородного дома Астрида. Предки Мист носили его в бесчисленных дуэлях.");
        AddRU(160, "Глаз Провидца", "Кольцо, способное видеть сквозь завесу снов. Прошлое, настоящее и будущее смешиваются.");
        AddRU(161, "Кольцо Первозданного Сна", "Выковано на заре снов. Содержит сущность первого сновидца.");
        AddRU(162, "Вечный Сон", "Кольцо, касающееся самого глубокого сна. Носители не боятся ни смерти, ни пробуждения.");
        
        // Amulets (163-183)
        AddRU(163, "Кулон Сновидца", "Простой кулон, носимым теми, кто только что вошёл в мир снов. Излучает слабую надежду.");
        AddRU(164, "Кулон Прикосновения Огня", "Кулон, тронутый огнём. Его тепло рассеивает холод и страх.");
        AddRU(165, "Алая Слеза", "Рубин в форме кровавой слезы. Его сила происходит от жертвоприношения.");
        AddRU(166, "Янтарный Амулет", "Янтарь, содержащий древнее существо. Его воспоминания укрепляют носителя.");
        AddRU(167, "Медаль Сумерек", "Медаль, мерцающая между светом и тьмой. Новые члены Веспер носят эти.");
        AddRU(168, "Ожерелье Звёздной Пыли", "Ожерелье, посыпанное кристальными звёздами. Смузи собирает их из упавших снов.");
        AddRU(169, "Изумрудный Глаз", "Зелёный самоцвет, способный видеть сквозь иллюзии. Кошмары не могут избежать его взгляда.");
        AddRU(170, "Сердце Духа Леса", "Самоцвет, содержащий благословение духов леса. Стражи Начии ценят эти.");
        AddRU(171, "Метка Лунного Арканум", "Метка из запрещённых архивов. Аурена рискует всем, изучая их.");
        AddRU(172, "Кристалл Бисмут", "Фрагмент чистой кристальной магии. Резонирует с энергией арканум.");
        AddRU(173, "Амулет Тлеющих Углей Пламени", "Запечатанная искра священного огня. Орден Веспер охраняет эти реликвии.");
        AddRU(174, "Кулон Клыка Кошмара", "Клык могучего кошмара, теперь трофей. Шелл носит один как напоминание.");
        AddRU(175, "Медаль Королевской Гвардии", "Медаль чести Королевской Гвардии. Ласерта завоевал много до своего изгнания.");
        AddRU(176, "Герб Астрида", "Благородный герб семьи Астрида. Наследие семьи Мист висит на этой цепи.");
        AddRU(177, "Священная Слеза Эль", "Слеза, пролитая самим Эль. Его свет защищает от самых тёмных кошмаров.");
        AddRU(178, "Первозданный Изумруд", "Самоцвет из первого леса. Содержит сны древних духов.");
        AddRU(179, "Глаз Повелителя Кошмаров", "Глаз побеждённого повелителя кошмаров. Видит все страхи и использует их.");
        AddRU(180, "Сердце Пламени", "Ядро самого священного огня. Только самые благочестивые могут носить его.");
        AddRU(181, "Оракул Провидца", "Амулет, способный видеть все возможные будущие. Реальность изгибается перед его пророчествами.");
        AddRU(182, "Компас Странника Пустоты", "Амулет, указывающий на пустоту. Те, кто следует за ним, никогда не возвращаются тем же путём.");
        AddRU(183, "Амулет Мидаса", "Легендарный амулет, тронутый сном золота. Побеждённые враги роняют дополнительное золото.");
        AddRU(478, "Собиратель Звёздной Пыли", "Легендарный амулет, собирающий кристаллизованную звёздную пыль с побеждённых врагов. Каждое убийство даёт драгоценную сущность снов (база 1-5, масштабируется с уровнем улучшения и силой монстра).");
        
        // Belts (184-203)
        AddRU(184, "Пояс Странника", "Простой тканевый пояс, носимым теми, кто бродит между снами. Якорит душу.");
        AddRU(185, "Верёвка Сновидца", "Верёвка, сплетённая из нитей снов. Мягко пульсирует с каждым ударом сердца.");
        AddRU(186, "Пояс Звёздной Пыли", "Пояс, украшенный кристальной звёздной пылью. Смузи продаёт подобные аксессуары.");
        AddRU(187, "Пояс Стража", "Прочный пояс, носимым стражами снов. Никогда не ослабляется, никогда не подводит.");
        AddRU(188, "Мистический Пояс", "Пояс, пропитанный слабой магией. Колет, когда приближаются кошмары.");
        AddRU(189, "Пояс Охотника", "Кожаный пояс с сумками для припасов. Необходим для долгих путешествий в снах.");
        AddRU(190, "Пояс Паломника", "Носится теми, кто ищет свет Эль. Предлагает утешение в самых тёмных снах.");
        AddRU(191, "Пояс Охотника на Кошмары", "Пояс, сделанный из побеждённых кошмаров. Их сущность укрепляет носителя.");
        AddRU(192, "Пояс Арканум", "Пояс из Лунного Общества Арканум. Усиливает магическую энергию.");
        AddRU(193, "Пояс Пламени", "Пояс, благословлённый Орденом Веспер. Горит огнём защиты.");
        AddRU(194, "Пояс Меча Дуэлянта", "Элегантный пояс, носимым благородными воинами Астрида. Несёт мечи и честь.");
        AddRU(195, "Верёвка Ткача Духов", "Пояс, сплетённый духами леса для Начии. Жужжит природной энергией.");
        AddRU(196, "Пояс Инструментов Убийцы", "Пояс со скрытыми отделениями. Контролёры Шелла используют их для оснащения своих инструментов.");
        AddRU(197, "Пояс Кобуры Револьвера", "Пояс, разработанный для огнестрельного оружия Ласерты. Быстрое выхватывание, более быстрая смерть.");
        AddRU(198, "Священный Пояс Эль", "Пояс, благословлённый самим Эль. Его свет якорит душу против всей тьмы.");
        AddRU(199, "Цепь Повелителя Кошмаров", "Пояс, выкованный из цепей повелителя кошмаров. Связывает страх с волей носителя.");
        AddRU(200, "Пояс Первозданного Сна", "Пояс из первого сна. Содержит эхо самого творения.");
        AddRU(201, "Пояс Корня Древа Мира", "Пояс, растущий из корней Древа Мира. Лес Начии благословляет его создание.");
        AddRU(202, "Наследие Астрида", "Семейный пояс семьи Астрида. Кровь Мист вплетена в его ткань.");
        AddRU(203, "Пояс Инквизитора Сумерек", "Пояс самого высокого уровня Веспер. Судит всех, кто стоит перед ним.");
        
        // Armor Sets - Bone Sentinel (204-215)
        AddRU(204, "Маска Костяного Стража", "Маска, вырезанная из черепа павшего кошмара. Её пустые глаза видят сквозь иллюзии.");
        AddRU(205, "Шлем Стража Духовного Мира", "Шлем, пропитанный духами леса Начии. Шёпоты защиты окружают носителя.");
        AddRU(206, "Корона Повелителя Кошмаров", "Корона, выкованная из завоёванных кошмаров. Носитель правит над самим страхом.");
        AddRU(207, "Жилет Костяного Стража", "Доспех из рёбер существ, умерших во снах. Их защита длится за пределами смерти.");
        AddRU(208, "Кираса Стража Духовного Мира", "Кираса, сплетённая из кристаллизованных снов. Движется и адаптируется к входящим ударам.");
        AddRU(209, "Кираса Повелителя Кошмаров", "Доспех, пульсирующий тёмной энергией. Кошмары склоняются перед его носителем.");
        AddRU(210, "Поножи Костяного Стража", "Поножи, усиленные костями кошмаров. Никогда не устают, никогда не уступают.");
        AddRU(211, "Поножи Стража Духовного Мира", "Поножи, благословлённые духами леса. Носитель движется как ветер сквозь листья.");
        AddRU(212, "Поножи Повелителя Кошмаров", "Поножи, пьющие тени. Тьма укрепляет каждый шаг.");
        AddRU(213, "Сапоги Костяного Стража", "Сапоги для бесшумной ходьбы в царствах снов. Мёртвые не издают звука.");
        AddRU(214, "Железные Сапоги Стража Духовного Мира", "Сапоги, не оставляющие следов в царствах снов. Идеальны для охотников на кошмары.");
        AddRU(215, "Сапоги Повелителя Кошмаров", "Сапоги, пересекающие миры. Реальность изгибается под каждым шагом.");
        
        // Aurena Armor (216-243)
        AddRU(216, "Шапка Ученика Аурены", "Простая шапка для новичков Лунного Арканум.");
        AddRU(217, "Мистический Тюрбан Аурены", "Тюрбан, пропитанный слабой лунной энергией.");
        AddRU(218, "Чарующая Корона Аурены", "Корона, усиливающая магическое сродство носителя.");
        AddRU(219, "Древняя Корона Аурены", "Древняя реликвия, передаваемая через поколения.");
        AddRU(220, "Повязка Сокола Аурены", "Повязка, благословлённая духами неба.");
        AddRU(221, "Корона Силы Аурены", "Мощная корона, излучающая силу арканум.");
        AddRU(222, "Лунная Корона Аурены", "Легендарная корона мастера Лунного Арканум.");
        AddRU(223, "Синий Халат Аурены", "Простой халат, носимый лунными магами.");
        AddRU(224, "Халат Мага Аурены", "Халат, ценимый практикующими магами.");
        AddRU(225, "Заклинательный Халат Аурены", "Халат, сотканный магическими нитями.");
        AddRU(226, "Просветляющий Халат Аурены", "Халат, направляющий лунную мудрость.");
        AddRU(227, "Саван Снов Аурены", "Саван, сотканный из кристаллизованных снов.");
        AddRU(228, "Королевский Чешуйчатый Халат Аурены", "Халат, подходящий лунной королевской семье.");
        AddRU(229, "Халат Ледяной Королевы Аурены", "Легендарный халат самой Ледяной Королевы.");
        AddRU(230, "Синие Поножи Аурены", "Простые поножи для начинающих магов.");
        AddRU(231, "Волокнистая Юбка Аурены", "Лёгкая юбка, позволяющая свободно двигаться.");
        AddRU(232, "Эльфийские Поножи Аурены", "Элегантные поножи, разработанные эльфами.");
        AddRU(233, "Просветляющие Поножи Аурены", "Поножи, благословлённые мудростью.");
        AddRU(234, "Ледниковая Юбка Аурены", "Юбка, пропитанная магией мороза.");
        AddRU(235, "Морозные Кюлоты Аурены", "Кюлоты, излучающие силу холода.");
        AddRU(236, "Великолепные Поножи Аурены", "Легендарные поножи несравненной элегантности.");
        AddRU(237, "Мягкие Лунные Тапочки", "Мягкая обувь, носимая аколитами Лунного Арканум. Смягчают каждый шаг лунным светом.");
        AddRU(238, "Сандалии Паломника", "Простые сандалии, благословлённые старейшинами Общества. Аурена всё ещё помнит их тепло.");
        AddRU(239, "Обувь Восточной Мистики", "Экзотическая обувь из далёких земель. Направляют целительную энергию через землю.");
        AddRU(240, "Милость Ходока по Снам", "Сапоги, идущие между бодрствующим и сновидческим мирами. Идеальны для целителей, ищущих потерянные души.");
        AddRU(241, "Сапоги Странника Душ", "Сапоги, пропитанные сущностью умерших целителей. Их мудрость направляет каждый шаг.");
        AddRU(242, "Сапоги Преследователя Душ", "Тёмные сапоги, выслеживающие раненых. Аурена использует их, чтобы найти нуждающихся в помощи... или тех, кто навредил ей.");
        AddRU(243, "Крылья Падшего Мудреца", "Легендарные сапоги, которые, как говорят, дают способность летать чистым сердцам. Изгнание Аурены доказало её квалификацию.");
        
        // Bismuth Armor (244-271)
        AddRU(244, "Конусная Шляпа Бисмут", "Простая шляпа для кристальных учеников.");
        AddRU(245, "Изумрудная Шляпа Бисмут", "Шляпа, инкрустированная изумрудными кристаллами.");
        AddRU(246, "Ледниковая Маска Бисмут", "Маска из замороженных кристаллов.");
        AddRU(247, "Маска Аларарим Бисмут", "Древняя маска кристальной силы.");
        AddRU(248, "Призматический Шлем Бисмут", "Шлем, преломляющий свет в радугу.");
        AddRU(249, "Отражающая Корона Бисмут", "Корона, отражающая магическую энергию.");
        AddRU(250, "Раскалённая Корона Бисмут", "Легендарная корона, горящая кристальным огнём.");
        AddRU(251, "Ледяной Халат Бисмут", "Халат, охлаждённый кристальной магией.");
        AddRU(252, "Халат Монаха Бисмут", "Простой халат для медитации.");
        AddRU(253, "Ледниковый Халат Бисмут", "Халат, замороженный в вечном инее.");
        AddRU(254, "Кристальная Броня Бисмут", "Броня, сформированная из живых кристаллов.");
        AddRU(255, "Призматическая Броня Бисмут", "Броня, искривляющая сам свет.");
        AddRU(256, "Замороженная Латная Броня Бисмут", "Латная броня вечного инея.");
        AddRU(257, "Священная Латная Броня Бисмут", "Легендарная броня, благословлённая кристальными богами.");
        AddRU(258, "Кольчужные Поножи Бисмут", "Базовые поножи с кристальными звеньями.");
        AddRU(259, "Волокнистые Поножи Бисмут", "Лёгкие поножи для кристальных магов.");
        AddRU(260, "Изумрудные Поножи Бисмут", "Поножи, украшенные изумрудными кристаллами.");
        AddRU(261, "Странные Штаны Бисмут", "Штаны, пульсирующие странной энергией.");
        AddRU(262, "Призматические Поножи Бисмут", "Поножи, сверкающие призматическим светом.");
        AddRU(263, "Поножи Аларарим Бисмут", "Древние поножи Аларарим.");
        AddRU(264, "Поножи Сокола Бисмут", "Легендарные поножи Ордена Сокола.");
        AddRU(265, "Тапочки Учёного", "Удобная обувь для долгой работы в библиотеках. Знание течёт через их подошвы.");
        AddRU(266, "Обёртки Аларарим", "Древние обёртки из потерянной цивилизации. Они шепчут забытые заклинания.");
        AddRU(267, "Ледяная Обувь", "Сапоги, вырезанные из вечного льда. Бисмут оставляет морозные узоры, где ходит.");
        AddRU(268, "Шаг Ледяного Цветка", "Элегантные сапоги, расцветающие ледяными кристаллами. Каждый шаг создаёт морозный сад.");
        AddRU(269, "Сапоги Кристального Резонанса", "Сапоги, усиливающие магическую энергию. Кристаллы жужжат неиспользованной силой.");
        AddRU(270, "Призматические Арканум Сапоги", "Сапоги, преломляющие свет в чистую магическую энергию. Реальность изгибается вокруг каждого шага.");
        AddRU(271, "Сапоги Странника Пустоты", "Легендарные сапоги, пересекающие саму пустоту. Бисмут видит пути, которые другие не могут вообразить.");
        
        // Lacerta Armor (272-299)
        AddRU(272, "Кожаный Шлем Ящера", "Базовый шлем для драконьих воинов.");
        AddRU(273, "Кольчужный Шлем Ящера", "Прочный кольчужный шлем.");
        AddRU(274, "Шлем Воина Ящера", "Шлем, носимый опытными воинами.");
        AddRU(275, "Шлем Драконьей Чешуи Ящера", "Шлем, выкованный из драконьей чешуи.");
        AddRU(276, "Королевский Шлем Ящера", "Шлем, подходящий королевской драконьей семье.");
        AddRU(277, "Элитный Шлем Драконьего Человека Ящера", "Элитный шлем Ордена Драконьего Человека.");
        AddRU(278, "Золотой Шлем Ящера", "Легендарный золотой шлем Короля Драконов.");
        AddRU(279, "Кожаная Броня Ящера", "Базовая броня для драконьих воинов.");
        AddRU(280, "Чешуйчатая Броня Ящера", "Броня, усиленная чешуёй.");
        AddRU(281, "Рыцарская Броня Ящера", "Тяжёлая броня для драконьих рыцарей.");
        AddRU(282, "Кольчуга Драконьей Чешуи Ящера", "Кольчуга, выкованная из драконьей чешуи.");
        AddRU(283, "Королевская Кольчуга Драконьего Человека Ящера", "Королевская кольчуга Ордена Драконьего Человека.");
        AddRU(284, "Латная Броня Сокола Ящера", "Латная броня Ордена Сокола.");
        AddRU(285, "Золотая Броня Ящера", "Легендарная золотая броня Короля Драконов.");
        AddRU(286, "Кожаные Поножи Ящера", "Базовые поножи для драконьих воинов.");
        AddRU(287, "Клёпаные Поножи Ящера", "Поножи, усиленные клёпками.");
        AddRU(288, "Рыцарские Поножи Ящера", "Тяжёлые поножи для драконьих рыцарей.");
        AddRU(289, "Поножи Драконьей Чешуи Ящера", "Поножи, выкованные из драконьей чешуи.");
        AddRU(290, "Королевские Поножи Ящера", "Поножи, подходящие королевской семье.");
        AddRU(291, "Великолепные Поножи Ящера", "Великолепные поножи мастерского мастерства.");
        AddRU(292, "Золотые Поножи Ящера", "Легендарные золотые поножи Короля Драконов.");
        AddRU(293, "Потёртые Кожаные Сапоги", "Простые сапоги, видевшие много битв. Они несут аромат приключений.");
        AddRU(294, "Залатанные Боевые Сапоги", "Сапоги, починенные бесчисленное количество раз. Каждая заплатка рассказывает историю выживания.");
        AddRU(295, "Сапоги Стального Воина", "Тяжёлые сапоги, выкованные для боя. Якорят Ласерту к земле во время битвы.");
        AddRU(296, "Поножи Стража", "Сапоги, носимые элитными стражами. Никогда не отступают, никогда не сдаются.");
        AddRU(297, "Поножи Драконьей Чешуи", "Сапоги из драконьей чешуи. Дают носителю стойкость дракона.");
        AddRU(298, "Сапоги Военачальника Драконьего Человека", "Сапоги древних драконьих генералов. Грозны на любом поле битвы.");
        AddRU(299, "Сапоги Золотого Чемпиона", "Легендарные сапоги величайшего воина эпохи. Судьба Ласерты ждёт.");
        
        // Mist Armor (300-327)
        AddRU(300, "Тюрбан Мист", "Простой тюрбан, скрывающий личность.");
        AddRU(301, "Лёгкий Тюрбан Мист", "Лёгкий, дышащий тюрбан.");
        AddRU(302, "Тюрбан Ночного Зрения Мист", "Тюрбан, усиливающий ночное зрение.");
        AddRU(303, "Повязка Молнии Мист", "Повязка, заряженная электрической энергией.");
        AddRU(304, "Капюшон Кобры Мист", "Капюшон, смертельный как змея.");
        AddRU(305, "Маска Адского Преследователя Мист", "Маска, выслеживающая добычу.");
        AddRU(306, "Тёмный Шёпот Мист", "Легендарная маска, шепчущая смерть.");
        AddRU(307, "Кожаная Упряжь Мист", "Лёгкая упряжь для ловкости.");
        AddRU(308, "Плащ Следопыта Мист", "Плащ для быстрого передвижения.");
        AddRU(309, "Туника Полуночи Мист", "Туника для ночных ударов.");
        AddRU(310, "Халат Молнии Мист", "Халат, заряженный молниями.");
        AddRU(311, "Броня Напряжения Мист", "Броня, пульсирующая электрической энергией.");
        AddRU(312, "Броня Мастера Лучника Мист", "Броня мастера лучника.");
        AddRU(313, "Плащ Тёмного Лорда Мист", "Легендарный плащ Тёмного Лорда.");
        AddRU(314, "Поножи Следопыта Мист", "Поножи, носимые следопытами.");
        AddRU(315, "Поножи Выживальщика Джунглей Мист", "Поножи выживальщика джунглей.");
        AddRU(316, "Саронг Полуночи Мист", "Саронг для ночных операций.");
        AddRU(317, "Поножи Молнии Мист", "Поножи, заряженные молниями.");
        AddRU(318, "Поножи Кузнечика Мист", "Поножи для поразительных прыжков.");
        AddRU(319, "Поножи Зелёного Демона Мист", "Поножи демонической скорости.");
        AddRU(320, "Поножи Странника Душ Мист", "Легендарные поножи, пересекающие миры.");
        AddRU(321, "Временные Разведывательные Сапоги", "Сапоги, собранные из фрагментов. Лёгкие и тихие, идеальны для разведки.");
        AddRU(322, "Сапоги Болотного Бегуна", "Водонепроницаемые сапоги для трудной местности. Мист знает каждую короткую дорогу.");
        AddRU(323, "Быстрый Странник", "Зачарованные сапоги, ускоряющие шаги носителя. Идеальны для тактики ударил-и-беги.");
        AddRU(324, "Сапоги Охотника на Оленя", "Сапоги, выслеживающие добычу.");
        AddRU(325, "Сапоги Удара Молнии", "Сапоги, заряженные энергией молнии.");
        AddRU(326, "Боевые Сапоги Ходока по Огню", "Сапоги, способные идти сквозь огонь.");
        AddRU(327, "Топчущий Страдания", "Легендарные сапоги, топчущие все страдания.");
        
        // Nachia Armor (328-355)
        AddRU(328, "Меховая Шапка Начии", "Простая шапка для лесных охотников.");
        AddRU(329, "Перьевой Головной Убор Начии", "Головной убор, украшенный перьями.");
        AddRU(330, "Маска Шамана Начии", "Маска духовной силы.");
        AddRU(331, "Капюшон Земли Начии", "Капюшон, благословлённый духами земли.");
        AddRU(332, "Шлем Природы Начии", "Шлем, пропитанный природной силой.");
        AddRU(333, "Корона Дерева Начии", "Корона, растущая из древнего дерева.");
        AddRU(334, "Корона Листа Начии", "Легендарная корона Хранителя Леса.");
        AddRU(335, "Меховая Броня Начии", "Броня из лесных существ.");
        AddRU(336, "Туземная Броня Начии", "Традиционная броня лесных племён.");
        AddRU(337, "Плащ Зелёного Дерева Начии", "Плащ, сплетённый из живых лоз.");
        AddRU(338, "Плащ Земли Начии", "Плащ, благословлённый духами земли.");
        AddRU(339, "Объятие Природы Начии", "Броня, объединённая с природой.");
        AddRU(340, "Броня Болотного Гнезда Начии", "Броня из глубоких болот.");
        AddRU(341, "Халат Листа Начии", "Легендарный халат Хранителя Леса.");
        AddRU(342, "Шорты из Мамонтового Меха Начии", "Тёплые шорты из мамонтового меха.");
        AddRU(343, "Кожаные Шорты Начии", "Традиционные охотничьи поножи.");
        AddRU(344, "Поножи Оленя Начии", "Поножи из оленьей кожи.");
        AddRU(345, "Поножи Земли Начии", "Поножи, благословлённые духами земли.");
        AddRU(346, "Поножи Листа Начии", "Поножи, сплетённые из магических листьев.");
        AddRU(347, "Набедренная Повязка Кабана-Человека Начии", "Набедренная повязка от могучего кабана-человека.");
        AddRU(348, "Поножи Кровавой Охоты Начии", "Легендарные поножи кровавой охоты.");
        AddRU(349, "Сапоги с Меховой Подкладкой", "Тёплые, удобные сапоги.");
        AddRU(350, "Сапоги из Барсучьей Кожи", "Сапоги из барсучьей кожи.");
        AddRU(351, "Сапоги Удара Кобры", "Сапоги, быстрые как змея.");
        AddRU(352, "Обёртки Лесного Преследователя", "Обёртки, бесшумные в лесу.");
        AddRU(353, "Сапоги Земного Охотника", "Сапоги, выслеживающие добычу.");
        AddRU(354, "Сапоги Следопыта Лихорадочного Цветка", "Сапоги, украшенные лихорадочными цветами. Дают лихорадочную скорость и смертельную точность.");
        AddRU(355, "Сапоги Кровавого Хищника", "Легендарные сапоги, пропитанные кровью бесчисленных охот. Начия стала высшим хищником.");
        
        // Shell Armor (356-383)
        AddRU(356, "Повреждённый Шлем Шелла", "Повреждённый шлем из подземного мира.");
        AddRU(357, "Сломанная Маска Шелла", "Сломанная маска, всё ещё дающая защиту.");
        AddRU(358, "Шлем Костяного Лорда Шелла", "Шлем, выкованный из останков Костяного Лорда.");
        AddRU(359, "Скелетный Шлем Шелла", "Шлем, сформированный из костей.");
        AddRU(360, "Шлем Смерти Шелла", "Шлем самой смерти.");
        AddRU(361, "Скелетный Страж Нокферату Шелла", "Шлем из костей вампира.");
        AddRU(362, "Демонический Шлем Шелла", "Легендарный шлем Демона-Лорда.");
        AddRU(363, "Погребальная Ткань Шелла", "Ткань из могилы.");
        AddRU(364, "Старый Плащ Шелла", "Старый плащ из древних времён.");
        AddRU(365, "Костяной Плащ Нокферату Шелла", "Плащ, сплетённый из костей.");
        AddRU(366, "Плащ Душ Шелла", "Плащ беспокойных душ.");
        AddRU(367, "Халат Смерти Шелла", "Халат смерти.");
        AddRU(368, "Халат Подземного Мира Шелла", "Халат из глубин.");
        AddRU(369, "Демоническая Броня Шелла", "Легендарная броня Демона-Лорда.");
        AddRU(370, "Повреждённый Фартук Шелла", "Повреждённый, но полезный фартук.");
        AddRU(371, "Юбка из Мутировавших Костей Шелла", "Юбка из мутировавших костей.");
        AddRU(372, "Шипастые Обёртки Нокферату Шелла", "Обёртки с шипами.");
        AddRU(373, "Телесный Страж Нокферату Шелла", "Защита из консервированной плоти.");
        AddRU(374, "Кровавые Поножи Шелла", "Поножи, пропитанные кровью.");
        AddRU(375, "Кровавый Странник Нокферату Шелла", "Поножи, идущие в крови.");
        AddRU(376, "Демонические Поножи Шелла", "Легендарные поножи Демона-Лорда.");
        AddRU(377, "Бронированные Поножи", "Тяжёлые металлические сапоги, дробящие всё под ногами. Защита - лучшее нападение.");
        AddRU(378, "Сапоги Мореплавателя", "Прочные сапоги, выдерживающие любой шторм. Якорят Шелла в потоках битвы.");
        AddRU(379, "Поножи Глубин", "Сапоги, выкованные в глубинах океана. Устойчивы к любому давлению.");
        AddRU(380, "Сапоги Дробящие Кости", "Сапоги, усиленные костями монстров. Каждый шаг - угроза.");
        AddRU(381, "Сапоги Магмовой Крепости", "Сапоги, выкованные в огне вулкана. Излучают жар, сжигающий атакующих.");
        AddRU(382, "Сапоги Топчущего Кровь", "Жестокие сапоги, оставляющие следы разрушения. Враги боятся сражаться с ними.");
        AddRU(383, "Сапоги Непоколебимого Стража", "Легендарные сапоги неразрушимого защитника. Шелл становится неподвижной крепостью.");
        
        // Vesper Armor (384-411)
        AddRU(384, "Шляпа Ведьмы Веспер", "Простая шляпа для ткачей снов.");
        AddRU(385, "Странный Капюшон Веспер", "Капюшон, шепчущий секреты.");
        AddRU(386, "Причудливый Капюшон Веспер", "Капюшон, пульсирующий странной энергией.");
        AddRU(387, "Капюшон Энергии Кошмаров Веспер", "Капюшон энергии кошмаров.");
        AddRU(388, "Обёртки Отчаяния Веспер", "Обёртки, питающиеся отчаянием.");
        AddRU(389, "Корона Тёмного Колдуна Веспер", "Корона тёмной магии.");
        AddRU(390, "Шляпа Ферунбрас Веспер", "Легендарная шляпа Мастера Тьмы.");
        AddRU(391, "Красный Халат Веспер", "Халат для начинающих магов снов.");
        AddRU(392, "Душевные Узы Веспер", "Узы, направляющие души.");
        AddRU(393, "Халат Энергии Веспер", "Халат, потрескивающий энергией.");
        AddRU(394, "Призрачная Юбка Веспер", "Юбка, сотканная из призрачной энергии.");
        AddRU(395, "Плащ Душ Веспер", "Плащ, содержащий души.");
        AddRU(396, "Обёртки Душ Веспер", "Обёртки пленённых душ.");
        AddRU(397, "Халат Арканум Дракона Веспер", "Легендарный халат Арканум Дракона.");
        AddRU(398, "Поножи Душ Веспер", "Поножи, пропитанные энергией душ.");
        AddRU(399, "Экзотические Поножи Веспер", "Экзотические поножи из далёких царств.");
        AddRU(400, "Душевные Поножи Веспер", "Поножи, направляющие силу душ.");
        AddRU(401, "Кровавые Штаны Веспер", "Штаны, окрашенные магией крови.");
        AddRU(402, "Магмовые Поножи Веспер", "Поножи, выкованные в магическом огне.");
        AddRU(403, "Поножи Мудрости Веспер", "Поножи древней мудрости.");
        AddRU(404, "Штаны Древних Веспер", "Легендарные штаны от Древних.");
        AddRU(405, "Тапочки Аколита", "Мягкие тапочки, носимые новыми членами храма. Несут молитвы в каждом шаге.");
        AddRU(406, "Храмовая Обувь", "Традиционная обувь слуг Эль. Якорят Веспер в её вере.");
        AddRU(407, "Сапоги Странного Монаха", "Сапоги, носимые монахами, изучающими запретные тексты. Знание и вера переплетаются.");
        AddRU(408, "Обёртки, Благословлённые Гномами", "Обёртки, зачарованные гномами-ремесленниками. Технология встречается с божественностью.");
        AddRU(409, "Тапочки из Вампирского Шёлка", "Элегантные тапочки, сотканные из вампирского шёлка. Вытягивают жизнь из самой земли.");
        AddRU(410, "Тапочки Убийцы Демонов", "Зелёные тапочки, благословлённые для уничтожения зла. Сжигают демонов каждым шагом.");
        AddRU(411, "Сапоги Изгоняющего Кошмары", "Легендарные сапоги, пересекающие кошмары для спасения невинных. Окончательная миссия Веспер.");
        
        // Yubar Armor (412-439)
        AddRU(412, "Племенная Маска Юбара", "Маска для племенных воинов.");
        AddRU(413, "Шлем Викинга Юбара", "Шлем для северных воинов.");
        AddRU(414, "Рогатый Шлем Юбара", "Шлем с устрашающими рогами.");
        AddRU(415, "Головной Убор Стойкого Икс Юбара", "Головной убор для племени Икс.");
        AddRU(416, "Головной Убор Страха Огня Юбара", "Головной убор, горящий страхом.");
        AddRU(417, "Шлем Стойкого Икс Юбара", "Шлем для воинов Икс.");
        AddRU(418, "Лик Апокалипсиса Юбара", "Легендарный лик Апокалипсиса.");
        AddRU(419, "Медвежья Шкура Юбара", "Броня от могучего медведя.");
        AddRU(420, "Плащ из Мамонтового Меха Юбара", "Плащ от мамонта.");
        AddRU(421, "Халат Стойкого Икс Юбара", "Халат для племени Икс.");
        AddRU(422, "Плащ Магмы Юбара", "Плащ, выкованный в магме.");
        AddRU(423, "Кираса Стойкого Икс Юбара", "Кираса для воинов Икс.");
        AddRU(424, "Латная Броня Лавы Юбара", "Латная броня из лавы.");
        AddRU(425, "Броня Огненного Гиганта Юбара", "Легендарная броня Огненного Гиганта.");
        AddRU(426, "Поножи Стойкого Икс Юбара", "Поножи для племени Икс.");
        AddRU(427, "Штаны из Мутировавшей Кожи Юбара", "Штаны из мутировавшей кожи.");
        AddRU(428, "Юбка Стойкого Икс Юбара", "Юбка для воинов Икс.");
        AddRU(429, "Дварфские Поножи Юбара", "Прочные дварфские поножи.");
        AddRU(430, "Сплавные Поножи Юбара", "Поножи, усиленные сплавом.");
        AddRU(431, "Латные Поножи Юбара", "Тяжёлые латные поножи.");
        AddRU(432, "Гномьи Поножи Юбара", "Легендарные поножи гномьего мастерства.");
        AddRU(433, "Временные Боевые Сапоги", "Сапоги, собранные из фрагментов поля боя. Юбар обходится тем, что находит.");
        AddRU(434, "Племенные Обёртки", "Традиционные обёртки народа Юбара. Они связывают его с предками.");
        AddRU(435, "Поножи Оленьего Воина", "Сапоги, украшенные оленьими рогами. Дают силу Короля Леса.");
        AddRU(436, "Сапоги Топчущего Клыки", "Жестокие сапоги с шипами в форме клыков. Топчут врагов под ногами.");
        AddRU(437, "Сапоги Кровавого Берсерка", "Кроваво-красные сапоги, разжигающие ярость Юбара. Боль становится силой.");
        AddRU(438, "Топчущий Души", "Сапоги, собирающие души павших врагов. Их сила растёт с каждой битвой.");
        AddRU(439, "Сапоги Чемпиона Возвращения Домой", "Легендарные сапоги, духовно приносящие Юбара домой. Его предки сражаются рядом с ним.");
        
        // Legendary Items (440-443)
        AddRU(440, "Крылатый Шлем Ходока по Снам", "Легендарный шлем, дающий ходокам по снам способность летать. Автоматически атакует ближайших врагов.");
        AddRU(441, "Латная Броня Магии Снов", "Легендарная броня, защищающая ходоков по снам всех царств. Автоматически нацеливается на ближайших врагов.");
        AddRU(442, "Поножи Глубин", "Легендарные поножи, выкованные в самых глубоких частях снов.");
        AddRU(443, "Сапоги Хождения по Воде", "Легендарные сапоги, позволяющие носителю ходить по воде. Сокровище, разыскиваемое ходоками по снам всех царств.");
        
        // Consumables (444-447)
        AddRU(444, "Тоник Сновидца", "Простое средство из трав мира бодрствования. Даже самый слабый сон начинается с капли надежды.");
        AddRU(445, "Эссенция Грёз", "Дистиллирована из воспоминаний о мирном сне. Те, кто её пьёт, чувствуют тепло забытых снов, омывающее их раны.");
        AddRU(446, "Ясная Жизненность", "Создана Смузи из звёздной пыли и осколков кошмаров. Жидкость мерцает светом тысячи спящих звёзд.");
        AddRU(447, "Эликсир Эль", "Священный напиток, благословлённый светом самого Эль. Орден Веспер ревностно хранит его рецепт, ибо он способен исцелить даже раны от самых тёмных кошмаров.");
        
        // Universal Legendary Belt (448)
        AddRU(448, "Пояс Бесконечных Снов", "Легендарный пояс, связывающий носителя с вечным циклом снов. Его сила растёт с каждым побеждённым кошмаром.");
        
        // Hero-Specific Legendary Belts (449-457)
        AddRU(449, "Пояс Ткача Жизни Оурены", "Пояс, выкованный из украденных исследований Оурены о жизненной силе. Он ткёт жизнь и смерть в идеальный баланс.");
        AddRU(450, "Кристаллический Пояс Бисмут", "Пояс из чистого кристалла, пульсирующий видением слепой девочки. Он видит то, чего не могут глаза.");
        AddRU(451, "Пояс Багрового Ока Лацерты", "Пояс, направляющий легендарный взгляд Лацерты. Ни одна цель не ускользнёт от его бдительной силы.");
        AddRU(452, "Пояс Чемпиона Дуэлянтов Мист", "Верховный пояс Дома Астрид. Честь Мист вплетена в каждую нить.");
        AddRU(453, "Пояс Мирового Древа Начи", "Пояс, выращенный из самого Мирового Древа. Ярость природы течёт через него.");
        AddRU(454, "Несокрушимая Цепь Хаска", "Пояс, выкованный из несокрушимой решимости. Решимость Хаска — его сила.");
        AddRU(455, "Пояс Теневого Убийцы Шелл", "Пояс совершенной тьмы. Манипуляторы Шелл никогда не могли сломить его волю.");
        AddRU(456, "Пояс Сумеречного Инквизитора Веспер", "Пояс высшего ранга Ордена Эль. Он судит кошмары и изгоняет тьму.");
        AddRU(457, "Пояс Предков-Чемпионов Юбара", "Пояс, призывающий предков Юбара. Их сила течёт через каждое волокно.");
        
        // Universal Legendary Ring (458)
        AddRU(458, "Кольцо Вечных Снов", "Легендарное кольцо, соединяющее носителя с бесконечным царством снов. Его сила превосходит все границы.");
        
        // Hero-Specific Legendary Rings (459-467)
        AddRU(459, "Кольцо Великого Мудреца Оурены", "Кольцо Великого Мудреца Лунного Арканума. Оурена взяла его как доказательство их лицемерия.");
        AddRU(460, "Кольцо Сердца Самоцвета Бисмут", "Кольцо чистой кристаллической магии. Оно пульсирует преобразованной сущностью слепой девочки.");
        AddRU(461, "Кольцо Королевского Палача Лацерты", "Кольцо элитного палача Королевской Гвардии. Лацерта заработал его в бесчисленных битвах.");
        AddRU(462, "Кольцо Наследия Астрид Мист", "Родовое кольцо Дома Астрид. Кровь Мист течёт через его металл.");
        AddRU(463, "Кольцо Хранителя Леса Начи", "Кольцо, благословлённое духами леса Начи. Сила природы — его сущность.");
        AddRU(464, "Кольцо Непоколебимой Воли Хаска", "Кольцо, выкованное из непоколебимой воли. Решимость Хаска — его сила.");
        AddRU(465, "Кольцо Идеальной Тени Шелл", "Кольцо абсолютной тьмы. Манипуляторы Шелл никогда не могли контролировать его силу.");
        AddRU(466, "Кольцо Священного Пламени Веспер", "Кольцо, содержащее чистую сущность Священного Пламени Эль. Оно изгоняет всю тьму.");
        AddRU(467, "Кольцо Племенного Чемпиона Юбара", "Кольцо, направляющее силу предков Юбара. Их ярость усиливает каждый удар.");
        
        // Universal Legendary Amulet (468)
        AddRU(468, "Амулет Бесконечных Снов", "Легендарный амулет, связывающий носителя с вечным циклом снов. Его сила превосходит все границы.");
        
        // Hero-Specific Legendary Amulets (469-477)
        AddRU(469, "Амулет Ткача Жизни Оурены", "Амулет, выкованный из украденных исследований Оурены о жизненной силе. Он ткёт жизнь и смерть в идеальный баланс.");
        AddRU(470, "Кристаллическое Сердце Бисмут", "Амулет из чистого кристалла, пульсирующий видением слепой девочки. Он видит то, чего не могут глаза.");
        AddRU(471, "Амулет Багрового Ока Лацерты", "Амулет, направляющий легендарный взгляд Лацерты. Ни одна цель не ускользнёт от его бдительной силы.");
        AddRU(472, "Амулет Чемпиона Дуэлянтов Мист", "Верховный амулет Дома Астрид. Честь Мист вплетена в каждый самоцвет.");
        AddRU(473, "Амулет Мирового Древа Начи", "Амулет, выращенный из самого Мирового Древа. Ярость природы течёт через него.");
        AddRU(474, "Несокрушимый Амулет Хаска", "Амулет, выкованный из несокрушимой решимости. Решимость Хаска — его сила.");
        AddRU(475, "Амулет Теневого Убийцы Шелл", "Амулет совершенной тьмы. Манипуляторы Шелл никогда не могли сломить его волю.");
        AddRU(476, "Амулет Сумеречного Инквизитора Веспер", "Амулет высшего ранга Ордена Эль. Он судит кошмары и изгоняет тьму.");
        AddRU(477, "Амулет Предков-Чемпионов Юбара", "Амулет, призывающий предков Юбара. Их сила течёт через каждый самоцвет.");
    }
    
    private static void AddRU(int id, string name, string desc)
    {
        _nameRU[id] = name;
        _descRU[id] = desc;
    }
    
    // ============================================================
    // JAPANESE TRANSLATIONS
    // ============================================================
    private static void InitializeJA()
    {
        _nameJA = new Dictionary<int, string>();
        _descJA = new Dictionary<int, string>();
        
        // Aurena Weapons (1-18)
        AddJA(1, "初心者の爪", "月のアルカナム協会の新メンバーに与えられたシンプルな爪。アウレナは追放後も自分のものを保持した。");
        AddJA(2, "学者の爪", "禁断のテキストで研がれた爪。残す傷跡が秘密を囁く。");
        AddJA(3, "月の爪", "月明かりの下で微かに光る爪。月のエネルギーを破壊的な一撃に導く。");
        AddJA(4, "アルカナムの剃刀", "治癒のルーンが刻まれた爪 - 今は治すのではなく引き裂くために使われる。");
        AddJA(5, "追放者の爪", "追放後に密かに鍛造された。各切り傷は協会が彼女から奪ったものを思い出させる。");
        AddJA(6, "異端者の握り", "かつて別の追放された賢者に属していた爪。その精神がアウレナの攻撃を導く。");
        AddJA(7, "犠牲の爪", "アウレナの生命力が衰えるにつれて鋭くなる爪。協会はこの研究を危険すぎると判断した。");
        AddJA(8, "霜に縛られた爪", "協会の最も深い保管庫の氷で凍らされた爪。その触感は肉と精神を麻痺させる。");
        AddJA(9, "月の引き裂き手", "禁断の呪文で強化された。与える各傷がアウレナの同盟者を癒す。");
        AddJA(10, "輝く爪", "盗まれた太陽光に祝福された爪。アウレナの光の魔法研究は太陽と月の両方のカルトを激怒させた。");
        AddJA(11, "燃え殻の爪", "同意した夢見る者から抽出された生命力で点火された爪。炎は決して消えない。");
        AddJA(12, "影の爪", "悪夢の本質に浸された爪。アウレナは闇が光ができないものを癒すことを学んだ。");
        AddJA(13, "氷河の刈り取り者", "永遠の氷から彫られた爪。敵の血を凍らせ、アウレナの生命力を保つ。");
        AddJA(14, "不死鳥の爪", "不死鳥の炎で鍛造され、アウレナ自身の血で焼き入れられた爪。消えない怒りで燃える。");
        AddJA(15, "虚無の引き裂き手", "現実そのものを引き裂くことができる爪。協会はアウレナが次に何を発見するかを恐れている。");
        AddJA(16, "天の爪", "純粋な星の光に浸透された爪。各打撃は神々が隠したい知識への祈りである。");
        AddJA(17, "大賢者の爪", "協会の大賢者の儀式用の爪。アウレナはそれらを彼らの偽善の証拠として使用する。");
        AddJA(18, "生命の織り手の爪", "アウレナの傑作 - 生命力を破壊せずに盗むことができる爪。協会はこれを異端と呼ぶ。");
        
        // Bismuth Weapons (19-36)
        AddJA(19, "空のグリモワール", "埋められるのを待つ空の呪文書。ビスマス自身のように、そのページは無限の可能性を含む。");
        AddJA(20, "見習いの書", "初心者向けの呪文書。ビスマスになった盲目の少女は指でそのページをなぞり、魔法を夢見た。");
        AddJA(21, "水晶のコーデックス", "ページが薄い宝石で作られた本。ビスマスの結晶の性質と共鳴する。");
        AddJA(22, "放浪者の日記", "見たことはないが常に知っていた盲目の少女の観察を記録する旅行日記。");
        AddJA(23, "宝石への導入", "結晶魔法の基本ガイド。その言葉はビスマスの虹色の体のように輝く。");
        AddJA(24, "盲目者の古典", "盲目者のために浮き彫り文字で書かれた本。ビスマスは触覚と記憶でそれを読む。");
        AddJA(25, "燃え殻の言葉の書", "ページが永遠に燃える本。炎はビスマスが感じるが、見ない色で話す。");
        AddJA(26, "氷の辞書", "永遠の氷に包まれた本。その氷のページは夢の始まり以前の知識を保存する。");
        AddJA(27, "輝く写本", "内側の光を放つ本。ビスマスになる前に、それは盲目の少女を闇を通して導いた。");
        AddJA(28, "影のグリモワール", "光を吸収する本。その暗いページはビスマスでさえ読むのを恐れる秘密を含む。");
        AddJA(29, "生きている呪文書", "盲目の少女との融合を選んだ意識的な本。一緒に彼らはより大きなものになった。");
        AddJA(30, "プリズムのコーデックス", "魔法を構成色に分解する本。ビスマスはそれが屈折する光を通して世界を見る。");
        AddJA(31, "虚無の年代記", "闇で失われた者の記憶を記録する本。ビスマスは彼らを称えるために彼らの物語を読む。");
        AddJA(32, "宝石の心のグリモワール", "盲目の少女と融合した元の呪文書。そのページは結晶の生命で脈動する。");
        AddJA(33, "眩しい光の書", "見る者を眩しくさせるほど眩しい本。ビスマスにとって、それは単なる熱である。");
        AddJA(34, "霜に縛られた年代記", "時間で凍った古代の本。その予言は少女のように歩く宝石について語る。");
        AddJA(35, "名前のない書", "名前のない本、視力のない少女のように。一緒に彼らは自分のアイデンティティを見つけた。");
        AddJA(36, "地獄の古典", "盲目が自分を定義することを拒否する夢見る者の情熱で燃える本。");
        
        // Lacerta Weapons (37-54)
        AddJA(37, "新兵のライフル", "王立衛兵の新兵に配布される標準武器。ラセルタは訓練の最初の週が終わる前にそれを習得した。");
        AddJA(38, "ハンターの長銃", "信頼できる狩猟用ライフル。ラセルタは衛兵に加わる前に、家族を養うために同様の銃を使っていた。");
        AddJA(39, "パトロールカービン", "コンパクトで信頼性が高い。王国の国境での長時間パトロールに最適。");
        AddJA(40, "狙撃手の銃", "精度を威力より重視する者のために設計されたバランスの取れたライフル。");
        AddJA(41, "黒色火薬のマスケット", "古いが信頼できる。黒色火薬の匂いがラセルタに最初の狩猟を思い出させる。");
        AddJA(42, "偵察連発銃", "偵察任務用の軽量ライフル。速度と隠密性が原始的な威力を上回る。");
        AddJA(43, "王立衛兵のライフル", "ラセルタが任務中に携帯していた武器。各傷跡が物語を語る。");
        AddJA(44, "狙撃手の誇り", "決して外さない者のための精密ライフル。ラセルタの深紅の目は他の者が見えないものを見る。");
        AddJA(45, "燃える長銃", "火炎弾頭を装填。敵は弾が命中した後も長く燃え続ける。");
        AddJA(46, "霜咬みカービン", "絶対零度まで冷却された弾丸を発射。標的は致命的一撃の前にカタツムリのように遅くなる。");
        AddJA(47, "夜のライフル", "暗闇で狩るための武器。その射撃は影のように静か。");
        AddJA(48, "夜明けのマスケット", "夜明けの光に祝福された。その射撃は闇と欺瞞を貫く。");
        AddJA(49, "深紅の目", "ラセルタの伝説的な視線にちなんで名付けられた。このライフルの視線から逃れる標的はない。");
        AddJA(50, "王立処刑者", "衛兵のエリートのために予約された。ラセルタは王国を救った後にこのライフルを得た。");
        AddJA(51, "太陽の大砲", "濃縮された光を発射するライフル。各発射はミニチュアの日の出。");
        AddJA(52, "虚無の狩人", "純粋な闇の弾丸を発射するライフル。標的は跡形もなく消える。");
        AddJA(53, "絶対零度", "存在の中で最も冷たい点。時間さえもその前に凍る。");
        AddJA(54, "ドラゴンの息", "爆発性の火炎弾を装填したライフル。王国の敵はその咆哮を恐れることを学んだ。");
        
        // Mist Weapons (55-70)
        AddJA(55, "訓練用レイピア", "ミストの若い頃の訓練用剣。その時でさえ、彼女はアストリッドのすべての教官を上回った。");
        AddJA(56, "貴族のレイピア", "ミストの貴族の血統にふさわしいレイピア。軽量、優雅、致命的。");
        AddJA(57, "決闘者のレイピア", "一対一の戦闘のために設計されたレイピア。ミストは決闘で一度も負けたことがない。");
        AddJA(58, "迅速なレイピア", "ミストの腕の延長のように軽い。");
        AddJA(59, "アストリッドのレイピア", "アストリッドの最高の鍛冶場で鍛造された。王国の工芸の象徴。");
        AddJA(60, "踊り子のレイピア", "戦闘を芸術として扱う者のためのレイピア。ミストの動きは詩的。");
        AddJA(61, "燃えるレイピア", "炎に囲まれたレイピア。ミストの怒りは彼女の決意と同じくらい熱い。");
        AddJA(62, "影のレイピア", "闇から打ち出すレイピア。敵はミストが動くのを見る前に倒れる。");
        AddJA(63, "霜咬みレイピア", "凍った鋼のレイピア。その触れは体と魂を麻痺させる。");
        AddJA(64, "カミラの恩寵", "ミストが大切にした人からの贈り物。彼女は彼らの記憶を称えるために戦う。");
        AddJA(65, "受け流しのレイピア", "防御と攻撃のために設計されたレイピア。ミストは各攻撃を機会に変える。");
        AddJA(66, "輝くレイピア", "内側の光を放つレイピア。それは嘘と影を貫く。");
        AddJA(67, "煉獄のレイピア", "消えない情熱で燃えるレイピア。ミストの決意は消すことができない。");
        AddJA(68, "真夜中のレイピア", "絶対的な闇で鍛造されたレイピア。それは死の静寂で打つ。");
        AddJA(69, "冬のレイピア", "永遠の氷から彫られたレイピア。その寒さは、戦闘におけるミストの集中力にのみ匹敵する。");
        AddJA(70, "不屈のレイピア", "アストリッドの最大の決闘者の伝説的なレイピア。ミストは無数の勝利でこの称号を獲得した。");
        
        // Nachia Weapons (71-88)
        AddJA(71, "若木の杖", "精霊の森からの若い枝。若木でさえナキアの呼びかけに応える。");
        AddJA(72, "精霊召喚者", "森の精霊の声を導くために彫られた杖。彼らはナキアに秘密をささやく。");
        AddJA(73, "森の枝", "まだ成長している生きた枝。森の魔法がその中を流れる。");
        AddJA(74, "野生の杖", "ナキア自身のように野生で予測不可能。精霊はその混沌のエネルギーを愛する。");
        AddJA(75, "守護者の杖", "森を守る者が携帯する。ナキアはこの責任を持って生まれた。");
        AddJA(76, "精霊界の杖", "物質世界と精霊世界を結ぶ杖。ナキアは両者の間を歩く。");
        AddJA(77, "霜木の杖", "永遠の冬で凍った杖。北の冷たい精霊がその呼びかけに応える。");
        AddJA(78, "影の根の杖", "森の最も深い部分で成長。影の精霊がその周りで踊る。");
        AddJA(79, "燃え殻の木の杖", "燃えることを決して止めない杖。火の精霊がその温かさに引き寄せられる。");
        AddJA(80, "太陽の祝福の杖", "夜明けの精霊に祝福された。その光は迷った魂を家に導く。");
        AddJA(81, "フェンリルの牙", "頂上に精霊の狼の牙が埋め込まれた杖。ナキアの忠実な仲間がその力を導く。");
        AddJA(82, "古い樫の杖", "千年の樫から彫られた。最も古い精霊はそれが植えられた時を覚えている。");
        AddJA(83, "世界樹の枝", "伝説の世界樹からの枝。すべての森の精霊がそれに頭を下げる。");
        AddJA(84, "精霊王の杖", "精霊王自身の杖。ナキアは森を守ることによってそれを獲得した。");
        AddJA(85, "永遠の霜の杖", "森の凍った心からの永遠の氷の杖。冬の精霊がその命令に従う。");
        AddJA(86, "不死鳥の止まり木", "火の精霊が巣を作る杖。その炎は破壊ではなく再生をもたらす。");
        AddJA(87, "輝く森の杖", "千の蛍の光を放つ杖。希望の精霊がその中で踊る。");
        AddJA(88, "虚無の守護者の杖", "世界の境界を守る杖。影の精霊がその持ち主を守る。");
        
        // Shell Weapons (89-106)
        AddJA(89, "初心者の刀", "剣の道を学ぶ者のための基本的な侍刀。シェルは数日でそれを習得した。");
        AddJA(90, "静寂の刀", "音を立てないように設計された侍刀。シェルの犠牲者は死の到来を決して聞かない。");
        AddJA(91, "暗殺者の刀", "細くて正確。シェルは正しい場所への一撃だけが必要。");
        AddJA(92, "殺しの刀", "効率的。実用的。無情。シェル自身のように。");
        AddJA(93, "狩人の刀", "狩りのための侍刀。シェルは標的が排除されるまで止まらない。");
        AddJA(94, "虚ろの刀", "シェルが感じる空虚にちなんで名付けられた。あるいは感じない。");
        AddJA(95, "処刑者の刀", "無数の命を終わらせる侍刀。シェルはそのどれにも何も感じない。");
        AddJA(96, "精密の刀", "弱点を利用するように調整された侍刀。シェルは最適な攻撃を計算する。");
        AddJA(97, "霜咬みの刀", "冷たい悪意の侍刀。その寒さはシェルの空虚な心に匹敵する。");
        AddJA(98, "浄化の刀", "暗い仕事のための明るい侍刀。シェルは皮肉を感じない。");
        AddJA(99, "影の刀", "影を飲む侍刀。シェルは見えずに動き、音もなく打つ。");
        AddJA(100, "灼熱の短剣", "内なる火で加熱された侍刀。シェルの標的は血を流す前に燃える。");
        AddJA(101, "完璧な殺人者の道具", "究極の殺人者の究極の道具。シェルはこの刀のために生まれた。");
        AddJA(102, "無情の刃", "決して鈍らない侍刀、決して止まらない追跡者によって振られる。");
        AddJA(103, "地獄の暗殺者", "地獄の火で鍛造された侍刀。それはシェルの唯一の目的の強度で燃える。");
        AddJA(104, "虚無の刀", "絶対的な闇の侍刀。シェルのように、それは終わらせるためにのみ存在する。");
        AddJA(105, "凍った刀", "永遠の氷の侍刀。その判決はシェルと同じくらい冷たく最終的。");
        AddJA(106, "輝く終焉", "明るい侍刀の究極の形態。シェルはエルの名において判決を執行する。");
        
        // Vesper Weapons (107-124)
        AddJA(107, "初心者の杖", "太陽の炎の騎士団の新メンバーのためのシンプルな杖。ヴェスパーはここから旅を始めた。");
        AddJA(108, "審判のハンマー", "エルの意志を執行するために使用される戦闘ハンマー。各打撃は神聖な判決。");
        AddJA(109, "火の守護者の棒", "聖なる火を守る者が携帯する武器。その打撃は不浄を浄化する。");
        AddJA(110, "見習いの盾", "騎士団の見習いのためのシンプルな盾。信仰が最良の防御。");
        AddJA(111, "審問官の盾", "審問官が携帯する盾。それは無数の異端者の没落を見てきた。");
        AddJA(112, "黄昏の防衛者", "光と闇の間を守る盾。");
        AddJA(113, "狂信者の粉砕ハンマー", "宗教的な狂信で燃える戦闘ハンマー。敵はその前に震える。");
        AddJA(114, "浄化者の重いハンマー", "悪を浄化するために使用される巨大な戦闘ハンマー。一撃で罪を粉砕する。");
        AddJA(115, "エルの審判", "エル自身によって個人的に祝福された戦闘ハンマー。その判決は上訴できない。");
        AddJA(116, "信仰の要塞", "破壊不可能な盾。信仰が消えない限り、それは壊れない。");
        AddJA(117, "聖なる炎の盾", "聖なる火に囲まれた盾。それに触れる悪は燃やされる。");
        AddJA(118, "異端審問の要塞", "高位審問官の盾。ヴェスパーは無情な奉仕によってそれを獲得した。");
        AddJA(119, "夜明けの破壊者", "闇を粉砕する伝説的な戦闘ハンマー。ヴェスパーはエルの怒りの化身を振る。");
        AddJA(120, "神聖な破壊者", "最高司祭によって祝福された戦闘ハンマー。その打撃は神聖な怒りの重みを運ぶ。");
        AddJA(121, "エルの右手", "騎士団の最も神聖な武器。ヴェスパーはエルの意志の選ばれた道具。");
        AddJA(122, "炎の要塞", "騎士団の究極の防御。エルの光がヴェスパーをあらゆる害から守る。");
        AddJA(123, "永遠の警戒", "揺るぎない盾。ヴェスパーのように、それは常に闇を見守る。");
        AddJA(124, "黄昏の聖騎士の誓い", "騎士団の指導者の盾。ヴェスパーは魂をエルに結びつけることを誓った。");
        
        // Yubar Weapons (125-142)
        AddJA(125, "新生の星", "ちょうど形成された星。ユバルはすべての星がそれほど小さかった時を覚えている。");
        AddJA(126, "宇宙の燃え殻", "星の火の破片。それは創造そのものの熱で燃える。");
        AddJA(127, "星塵の球", "形作られるのを待っている圧縮された星塵。ユバルはそのような物質で銀河を織る。");
        AddJA(128, "夢の触媒", "夢のエネルギーを増幅する球。ユバルの最初の実験から。");
        AddJA(129, "星雲の核", "遠い星雲の心。ユバルは宇宙の織物からそれを摘み取った。");
        AddJA(130, "天体の種", "星になる種。ユバルは無数のそのような種を育てる。");
        AddJA(131, "超新星の破片", "爆発した星の破片。ユバルはその死を目撃し、その力を保存した。");
        AddJA(132, "重力の井戸", "重力を圧縮する球。空間自体がその周りで曲がる。");
        AddJA(133, "虚無の特異点", "無限の密度の点。ユバルはそれを使って世界を創造し破壊する。");
        AddJA(134, "太陽のコロナ", "太陽の外層大気の破片。それは星の怒りで燃える。");
        AddJA(135, "凍った彗星", "捕獲された彗星の核。それは宇宙の深部からの秘密を運ぶ。");
        AddJA(136, "星の炉", "星の核のミニチュア。ユバルはそれを使って新しい現実を鍛造する。");
        AddJA(137, "ビッグバンの遺物", "創造のビッグバンの破片。それは宇宙の起源を含む。");
        AddJA(138, "宇宙の織機", "現実を織る道具。ユバルはそれを使って運命を再形成する。");
        AddJA(139, "輝く創造", "創造そのものの光。ユバルはそれを使って最初の星々を生み出した。");
        AddJA(140, "エントロピーの心", "宇宙の崩壊の本質。すべてが終わり、ユバルがいつかを決める。");
        AddJA(141, "絶対零度", "存在の中で最も冷たい点。時間さえもその前に凍る。");
        AddJA(142, "大災害の核", "宇宙の破壊の心。ユバルは危機的な瞬間にのみそれを解放する。");
        
        // Rings (143-162)
        AddJA(143, "夢見る者の指輪", "夢の世界に初めて入る者が着けるシンプルな指輪。それは弱い希望を放つ。");
        AddJA(144, "星塵の指輪", "凝縮された星塵から鍛造された。夢見る者は危険な時にそれがより明るく輝くと言う。");
        AddJA(145, "眠りの印", "平和な眠りのマークが刻まれている。着用者は傷からより速く回復する。");
        AddJA(146, "精霊の指輪", "さまよう精霊を捕らえる指輪。彼らの光が暗い夢を通る道を導く。");
        AddJA(147, "放浪者の指輪", "夢の領域の放浪者が着ける。それは無数の忘れられた道を見てきた。");
        AddJA(148, "悪夢の破片の指輪", "倒された悪夢の破片を含む指輪。その闇は勇敢な者を強化する。");
        AddJA(149, "記憶の守護者", "貴重な記憶を保存する指輪。スムージーは自分の店で同様のものを売っている。");
        AddJA(150, "月のアルカナムの指輪", "月のアルカナム協会からの指輪。アウレナは追放前に同様のものを着けていた。");
        AddJA(151, "精霊の森の印", "ナキアの森の精霊に祝福された指輪。自然の力がその中を流れる。");
        AddJA(152, "王立衛兵のバッジ", "ラセルタが任務中に着けていた指輪。それはまだ責任の重みを運ぶ。");
        AddJA(153, "決闘者の栄光", "貴族の決闘者の間で受け継がれる指輪。ミストの家族はそのような指輪を大切にする。");
        AddJA(154, "聖なる炎の燃え殻の護符", "封印された聖なる炎の火花。ヴェスパーの騎士団がこれらの遺物を守る。");
        AddJA(155, "呪文書の破片", "ビスマスの結晶の魔法で作られた指輪。それはアルカナムのエネルギーでうなる。");
        AddJA(156, "狩人の印", "獲物を追跡する指輪。シェルのコントローラーはそれらを使って自分の道具を監視する。");
        AddJA(157, "エルの祝福", "エル自身によって祝福された指輪。その光は悪夢を追い散らし、負傷者を癒す。");
        AddJA(158, "悪夢の領主の印", "倒された悪夢の領主から取られた。その闇の力は腐敗し強化する。");
        AddJA(159, "アストリッドの遺産", "アストリッドの貴族の家からの指輪。ミストの祖先は無数の決闘でそれを着けていた。");
        AddJA(160, "夢見る者の目", "夢のベールを通して見ることができる指輪。過去、現在、未来が混ざり合う。");
        AddJA(161, "原初の夢の指輪", "夢の夜明けに鍛造された。それは最初の夢見る者の本質を含む。");
        AddJA(162, "永遠の眠り", "最も深い眠りに触れる指輪。着用者は死も目覚めも恐れない。");
        
        // Amulets (163-183)
        AddJA(163, "夢見る者のペンダント", "夢の世界に初めて入る者が着けるシンプルなペンダント。それは弱い希望を放つ。");
        AddJA(164, "火の触れのペンダント", "火に触れられたペンダント。その温かさが寒さと恐怖を追い散らす。");
        AddJA(165, "深紅の涙", "血の涙の形をしたルビー。その力は犠牲から来る。");
        AddJA(166, "琥珀の護符", "古代の生物を含む琥珀。その記憶が着用者を強化する。");
        AddJA(167, "黄昏のメダル", "光と闇の間で点滅するメダル。ヴェスパーの新メンバーがこれらを着ける。");
        AddJA(168, "星塵のネックレス", "結晶の星で散りばめられたネックレス。スムージーは落ちた夢からそれらを集める。");
        AddJA(169, "エメラルドの目", "幻影を通して見ることができる緑の宝石。悪夢はその視線から逃れることができない。");
        AddJA(170, "森の精霊の心", "森の精霊の祝福を含む宝石。ナキアの守護者がこれらを大切にする。");
        AddJA(171, "月のアルカナムの印", "禁止されたアーカイブからの印。アウレナはそれらを研究するためにすべてを危険にさらす。");
        AddJA(172, "ビスマスの水晶", "結晶の純粋な魔法の破片。それはアルカナムのエネルギーと共鳴する。");
        AddJA(173, "聖なる炎の燃え殻の護符", "封印された聖なる炎の火花。ヴェスパーの騎士団がこれらの遺物を守る。");
        AddJA(174, "悪夢の牙のペンダント", "強力な悪夢からの牙、今はトロフィー。シェルは思い出として一つを着けている。");
        AddJA(175, "王立衛兵のメダル", "王立衛兵からの名誉のメダル。ラセルタは追放前に多くを獲得した。");
        AddJA(176, "アストリッドの紋章", "アストリッドの家族の貴族の紋章。ミストの家族の遺産がこの鎖に掛かっている。");
        AddJA(177, "エルの聖なる涙", "エル自身によって流された涙。その光が最も暗い悪夢から守る。");
        AddJA(178, "原初のエメラルド", "最初の森からの宝石。それは古代の精霊の夢を含む。");
        AddJA(179, "悪夢の領主の目", "倒された悪夢の領主の目。それはすべての恐怖を見て利用する。");
        AddJA(180, "炎の心", "聖なる炎そのものの核。最も敬虔な者だけがそれを着けることができる。");
        AddJA(181, "夢見る者の神託", "すべての可能な未来を見ることができる護符。現実がその予言の前に曲がる。");
        AddJA(182, "虚無の歩行者の羅針盤", "虚無を指す護符。それに従う者は決して同じ方法で戻らない。");
        AddJA(183, "ミダスの護符", "金の夢に触れられた伝説的な護符。倒された敵は追加の金を落とす。");
        
        // Belts (184-203)
        AddJA(184, "放浪者のベルト", "夢の間をさまよう者が着けるシンプルな布のベルト。それは魂を固定する。");
        AddJA(185, "夢見る者の縄", "夢の糸で織られた縄。それは各心拍で軽く脈動する。");
        AddJA(186, "星塵のバックル", "結晶の星塵で飾られたベルト。スムージーは同様のアクセサリーを売っている。");
        AddJA(187, "見張りのベルト", "夢の守護者が着ける堅牢なベルト。それは決して緩まず、決して失敗しない。");
        AddJA(188, "神秘的なベルト", "弱い魔法で注入されたベルト。悪夢が近づくと刺す。");
        AddJA(189, "狩人のベルト", "補給袋付きの革のベルト。夢の中での長い旅に不可欠。");
        AddJA(190, "巡礼者のベルト", "エルの光を求める者が着ける。それは最も暗い夢の中で慰めを提供する。");
        AddJA(191, "悪夢の狩人のベルト", "倒された悪夢で作られたベルト。彼らの本質が着用者を強化する。");
        AddJA(192, "アルカナムのベルト", "月のアルカナム協会からのベルト。それは魔法のエネルギーを増幅する。");
        AddJA(193, "聖なる炎のベルト", "ヴェスパーの騎士団によって祝福されたベルト。それは保護の火で燃える。");
        AddJA(194, "決闘者の剣帯", "アストリッドの貴族の戦士が着ける優雅なベルト。それは剣と名誉を運ぶ。");
        AddJA(195, "精霊の織り手の縄", "森の精霊がナキアのために織ったベルト。それは自然のエネルギーでうなる。");
        AddJA(196, "暗殺者の道具ベルト", "隠された区画付きのベルト。シェルのコントローラーはそれらを使って自分の道具を装備する。");
        AddJA(197, "ガンマンのホルスター", "ラセルタの火器のために設計されたベルト。素早い引き抜き、より速い殺し。");
        AddJA(198, "エルの聖なるベルト", "エル自身によって祝福されたベルト。その光が魂をすべての闇に対して固定する。");
        AddJA(199, "悪夢の領主の鎖", "悪夢の領主の鎖から鍛造されたベルト。それは恐怖を着用者の意志に結びつける。");
        AddJA(200, "原初の夢のベルト", "最初の夢からのベルト。それは創造そのもののこだまを含む。");
        AddJA(201, "世界樹の根のベルト", "世界樹の根から成長するベルト。ナキアの森がその創造を祝福した。");
        AddJA(202, "アストリッドの家宝", "アストリッドの家族の家系のベルト。ミストの血がその織物に織り込まれている。");
        AddJA(203, "黄昏の審問官のベルト", "ヴェスパーの最高レベルのベルト。それはその前に立つすべての人を審判する。");
        
        // Armor Sets - Bone Sentinel (204-215)
        AddJA(204, "骨の見張りのマスク", "倒された悪夢の頭蓋骨から彫られたマスク。その空の目が幻影を通して見る。");
        AddJA(205, "精霊界の守護者のヘルム", "ナキアの森の精霊で注入されたヘルム。保護のささやきが着用者を囲む。");
        AddJA(206, "悪夢の領主の王冠", "征服された悪夢から鍛造された王冠。着用者が恐怖そのものを支配する。");
        AddJA(207, "骨の見張りのベスト", "夢の中で死んだ生物の肋骨からの鎧。彼らの保護は死を超えて続く。");
        AddJA(208, "精霊界の守護者の胸当て", "結晶化した夢で織られた胸当て。それは動き、来る打撃に適応する。");
        AddJA(209, "悪夢の領主の胸当て", "闇のエネルギーで脈動する鎧。悪夢がその着用者の前に頭を下げる。");
        AddJA(210, "骨の見張りのすね当て", "悪夢の骨で強化された脚の鎧。彼らは決して疲れず、決して譲らない。");
        AddJA(211, "精霊界の守護者の脚の鎧", "森の精霊によって祝福された脚の鎧。着用者が葉を通る風のように動く。");
        AddJA(212, "悪夢の領主の脚の鎧", "影を飲む脚の鎧。闇が各歩みに力を与える。");
        AddJA(213, "骨の見張りのブーツ", "夢の領域で静かに歩くためのブーツ。死者は音を立てない。");
        AddJA(214, "精霊界の守護者の鉄のブーツ", "夢の領域に痕跡を残さないブーツ。悪夢の狩人に最適。");
        AddJA(215, "悪夢の領主のブーツ", "世界を横断するブーツ。現実が各歩みの下で曲がる。");
        
        // Aurena Armor (216-243)
        AddJA(216, "アウレナの見習いの帽子", "月のアルカナムの初心者が着けるシンプルな帽子。");
        AddJA(217, "アウレナの神秘的なターバン", "弱い月光のエネルギーで注入されたターバン。");
        AddJA(218, "アウレナの魅惑的な王冠", "着用者の魔法の親和性を高める王冠。");
        AddJA(219, "アウレナの古い王冠", "世代を通じて受け継がれた古代の遺物。");
        AddJA(220, "アウレナの鷹のヘッドバンド", "空の精霊によって祝福されたヘッドバンド。");
        AddJA(221, "アウレナの力の王冠", "アルカナムの力を放つ強力な王冠。");
        AddJA(222, "アウレナの月光の王冠", "月のアルカナムのマスターの伝説的な王冠。");
        AddJA(223, "アウレナの青いローブ", "月の魔術師が着けるシンプルなローブ。");
        AddJA(224, "アウレナの魔術師のローブ", "実践的な魔術師が愛するローブ。");
        AddJA(225, "アウレナの呪文を織ったローブ", "魔法の糸で織られたローブ。");
        AddJA(226, "アウレナの啓発のローブ", "月光の知恵を導くローブ。");
        AddJA(227, "アウレナの夢の包帯", "結晶化した夢で織られた包帯。");
        AddJA(228, "アウレナの王立の鱗のローブ", "月の王室に適したローブ。");
        AddJA(229, "アウレナの氷の女王のローブ", "氷の女王自身の伝説的なローブ。");
        AddJA(230, "アウレナの青いすね当て", "志望の魔術師が着けるシンプルなすね当て。");
        AddJA(231, "アウレナの繊維のスカート", "自由な動きを可能にする軽量のスカート。");
        AddJA(232, "アウレナのエルフのすね当て", "エルフによって設計された優雅なすね当て。");
        AddJA(233, "アウレナの啓発のすね当て", "知恵によって祝福されたすね当て。");
        AddJA(234, "アウレナの氷河のスカート", "霜の魔法で注入されたスカート。");
        AddJA(235, "アウレナの霜のキュロット", "寒さの力を放つキュロット。");
        AddJA(236, "アウレナの華麗なすね当て", "比類のない優雅さの伝説的なすね当て。");
        AddJA(237, "柔らかい月光のスリッパ", "月のアルカナムの見習いが着ける柔らかい靴。彼らは各歩みを月光で緩衝する。");
        AddJA(238, "巡礼者のサンダル", "協会の長老によって祝福されたシンプルなサンダル。アウレナはまだ彼らの温かさを覚えている。");
        AddJA(239, "東洋の神秘の靴", "遠い国からの異国の靴。彼らは大地を通して治癒のエネルギーを導く。");
        AddJA(240, "夢の歩行者の恩寵", "目覚めた世界と夢の世界の間を歩くブーツ。迷った魂を探す治癒者に最適。");
        AddJA(241, "魂の歩行者のブーツ", "亡くなった治癒者の本質で注入されたブーツ。彼らの知恵が各歩みを導く。");
        AddJA(242, "魂の追跡者のブーツ", "負傷者を追跡する暗いブーツ。アウレナはそれらを使って助けを必要とする人を見つける...または彼女を傷つけた人。");
        AddJA(243, "堕落した賢者の翼", "純粋な心に飛行能力を与えると言われている伝説的なブーツ。アウレナの追放が彼女の資格を証明した。");
        
        // Bismuth Armor (244-271)
        AddJA(244, "ビスマスの円錐帽子", "結晶の見習いのためのシンプルな帽子。");
        AddJA(245, "ビスマスのエメラルドの帽子", "エメラルドの結晶で装飾された帽子。");
        AddJA(246, "ビスマスの氷河のマスク", "凍った結晶で作られたマスク。");
        AddJA(247, "ビスマスのアラハリムのマスク", "結晶の力の古代のマスク。");
        AddJA(248, "ビスマスのプリズムのヘルム", "光を虹に屈折させるヘルム。");
        AddJA(249, "ビスマスの反射する王冠", "魔法のエネルギーを反射する王冠。");
        AddJA(250, "ビスマスの白熱の王冠", "結晶の火で燃える伝説的な王冠。");
        AddJA(251, "ビスマスの氷のローブ", "結晶の魔法によって冷却されたローブ。");
        AddJA(252, "ビスマスの僧侶のローブ", "瞑想のためのシンプルなローブ。");
        AddJA(253, "ビスマスの氷河のローブ", "永遠の霜で凍ったローブ。");
        AddJA(254, "ビスマスの結晶の鎧", "生きた結晶から形成された鎧。");
        AddJA(255, "ビスマスのプリズムの鎧", "光そのものを曲げる鎧。");
        AddJA(256, "ビスマスの凍ったプレートの鎧", "永遠の霜のプレートの鎧。");
        AddJA(257, "ビスマスの聖なるプレートの鎧", "結晶の神々によって祝福された伝説的な鎧。");
        AddJA(258, "ビスマスのチェーンメイルのすね当て", "結晶のリンク付きの基本的なすね当て。");
        AddJA(259, "ビスマスの繊維のすね当て", "結晶の魔術師のための軽量のすね当て。");
        AddJA(260, "ビスマスのエメラルドのすね当て", "エメラルドの結晶で飾られたすね当て。");
        AddJA(261, "ビスマスの奇妙なズボン", "奇妙なエネルギーで脈動するズボン。");
        AddJA(262, "ビスマスのプリズムのすね当て", "プリズムの光で輝くすね当て。");
        AddJA(263, "ビスマスのアラハリムのすね当て", "アラハリムの古代のすね当て。");
        AddJA(264, "ビスマスの鷹のすね当て", "鷹の騎士団の伝説的なすね当て。");
        AddJA(265, "学者のスリッパ", "図書館での長時間の仕事のための快適な靴。知識がその靴底を通って流れる。");
        AddJA(266, "アラハリムの包帯", "失われた文明からの古代の包帯。彼らは忘れられた呪文をささやく。");
        AddJA(267, "氷の靴", "永遠の氷から彫られたブーツ。ビスマスが歩くところに霜のパターンを残す。");
        AddJA(268, "霜の花の歩み", "氷の結晶で咲く優雅なブーツ。各歩みが霜の庭を作る。");
        AddJA(269, "結晶の共鳴のブーツ", "魔法のエネルギーを増幅するブーツ。結晶が未開発の力でうなる。");
        AddJA(270, "プリズムのアルカナムのブーツ", "光を純粋な魔法のエネルギーに屈折させるブーツ。現実が各歩みの周りで曲がる。");
        AddJA(271, "虚無の歩行者のブーツ", "虚無そのものを横断する伝説的なブーツ。ビスマスは他の者が想像できない道を見ることができる。");
        
        // Lacerta Armor (272-299)
        AddJA(272, "リザードマンの革のヘルム", "ドラゴン戦士のための基本的なヘルム。");
        AddJA(273, "リザードマンのチェーンメイルのヘルム", "堅牢なチェーンメイルのヘルム。");
        AddJA(274, "リザードマンの戦士のヘルム", "経験豊富な戦士が着けるヘルム。");
        AddJA(275, "リザードマンのドラゴンスケールのヘルム", "ドラゴンスケールから鍛造されたヘルム。");
        AddJA(276, "リザードマンの王立のヘルム", "ドラゴンの王室に適したヘルム。");
        AddJA(277, "リザードマンのエリートのドラゴンマンのヘルム", "ドラゴンマンの騎士団のエリートのヘルム。");
        AddJA(278, "リザードマンの金のヘルム", "ドラゴン王の伝説的な金のヘルム。");
        AddJA(279, "リザードマンの革の鎧", "ドラゴン戦士のための基本的な鎧。");
        AddJA(280, "リザードマンのスケールの鎧", "スケールで強化された鎧。");
        AddJA(281, "リザードマンの騎士の鎧", "ドラゴン騎士の重い鎧。");
        AddJA(282, "リザードマンのドラゴンスケールのチェーンメイル", "ドラゴンスケールから鍛造されたチェーンメイル。");
        AddJA(283, "リザードマンの王立のドラゴンマンのチェーンメイル", "ドラゴンマンの騎士団の王立のチェーンメイル。");
        AddJA(284, "リザードマンの鷹のプレートの鎧", "鷹の騎士団のプレートの鎧。");
        AddJA(285, "リザードマンの金の鎧", "ドラゴン王の伝説的な金の鎧。");
        AddJA(286, "リザードマンの革のすね当て", "ドラゴン戦士のための基本的なすね当て。");
        AddJA(287, "リザードマンのリベット付きすね当て", "リベットで強化されたすね当て。");
        AddJA(288, "リザードマンの騎士のすね当て", "ドラゴン騎士の重いすね当て。");
        AddJA(289, "リザードマンのドラゴンスケールのすね当て", "ドラゴンスケールから鍛造されたすね当て。");
        AddJA(290, "リザードマンの王冠のすね当て", "王室に適したすね当て。");
        AddJA(291, "リザードマンの華麗なすね当て", "マスタークラフトの華麗なすね当て。");
        AddJA(292, "リザードマンの金のすね当て", "ドラゴン王の伝説的な金のすね当て。");
        AddJA(293, "すり減った革のブーツ", "多くの戦いを見てきたシンプルなブーツ。彼らは冒険の香りを運ぶ。");
        AddJA(294, "パッチの戦闘ブーツ", "無数の回修理されたブーツ。各パッチが生存の物語を語る。");
        AddJA(295, "鋼鉄の戦士のブーツ", "戦闘のために鍛造された重いブーツ。彼らは戦闘中にラセルタを地面に固定する。");
        AddJA(296, "守護者のすね当て", "エリートの保護者が着けるブーツ。彼らは決して後退せず、決して降伏しない。");
        AddJA(297, "ドラゴンスケールのすね当て", "ドラゴンスケールで作られたブーツ。彼らは着用者にドラゴンの強靭さを与える。");
        AddJA(298, "ドラゴンマンの軍閥のブーツ", "古代のドラゴンマンの将軍のブーツ。彼らはあらゆる戦場で恐ろしい。");
        AddJA(299, "金のチャンピオンのブーツ", "時代の最大の戦士が着ける伝説的なブーツ。ラセルタの運命が待っている。");
        
        // Mist Armor (300-327)
        AddJA(300, "ミストのターバン", "アイデンティティを隠すシンプルなターバン。");
        AddJA(301, "ミストの軽量ターバン", "軽量で通気性のあるターバン。");
        AddJA(302, "ミストの暗視ターバン", "夜間視力を向上させるターバン。");
        AddJA(303, "ミストの稲妻のヘッドバンド", "電気エネルギーで満たされたヘッドバンド。");
        AddJA(304, "ミストのコブラのフード", "蛇のように致命的なフード。");
        AddJA(305, "ミストの地獄の追跡者のマスク", "獲物を追跡するマスク。");
        AddJA(306, "ミストの闇のささやき", "死をささやく伝説的なマスク。");
        AddJA(307, "ミストの革のハーネス", "敏捷性のための軽量のハーネス。");
        AddJA(308, "ミストのレンジャーのマント", "素早い動きのためのマント。");
        AddJA(309, "ミストの真夜中のチュニック", "夜間の打撃のためのチュニック。");
        AddJA(310, "ミストの稲妻のローブ", "稲妻で満たされたローブ。");
        AddJA(311, "ミストの電圧の鎧", "電気エネルギーで脈動する鎧。");
        AddJA(312, "ミストのマスターアーチャーの鎧", "アーチャーのマスターの鎧。");
        AddJA(313, "ミストの闇の領主のマント", "闇の領主の伝説的なマント。");
        AddJA(314, "ミストのレンジャーのすね当て", "レンジャーが着けるすね当て。");
        AddJA(315, "ミストのジャングルサバイバーのすね当て", "ジャングルサバイバーのすね当て。");
        AddJA(316, "ミストの真夜中のサロン", "真夜中の作戦のためのサロン。");
        AddJA(317, "ミストの稲妻のすね当て", "稲妻で満たされたすね当て。");
        AddJA(318, "ミストのバッタのすね当て", "驚くべき跳躍のためのすね当て。");
        AddJA(319, "ミストの緑の悪魔のすね当て", "悪魔の速度のすね当て。");
        AddJA(320, "ミストの魂の歩行者のすね当て", "世界の間を歩く伝説的なすね当て。");
        AddJA(321, "一時的な偵察ブーツ", "破片から組み立てられたブーツ。軽量で静か、偵察に最適。");
        AddJA(322, "沼地のランナーのブーツ", "困難な地形を横断する防水ブーツ。ミストはすべての近道を知っている。");
        AddJA(323, "迅速な歩行者", "着用者の歩みを加速する魔法のブーツ。ヒットアンドラン戦術に最適。");
        AddJA(324, "雄鹿の狩人のブーツ", "獲物を追跡するブーツ。");
        AddJA(325, "稲妻のストライカーのブーツ", "稲妻のエネルギーで満たされたブーツ。");
        AddJA(326, "火の歩行者の戦闘ブーツ", "火を通して歩くことができるブーツ。");
        AddJA(327, "苦難の踏みつけ", "すべての苦難を踏みつける伝説的なブーツ。");
        
        // Nachia Armor (328-355)
        AddJA(328, "ナキアの毛皮の帽子", "森の狩人のためのシンプルな帽子。");
        AddJA(329, "ナキアの羽の頭飾り", "羽で飾られた頭飾り。");
        AddJA(330, "ナキアのシャーマンのマスク", "精神的な力のマスク。");
        AddJA(331, "ナキアの大地のフード", "大地の精霊によって祝福されたフード。");
        AddJA(332, "ナキアの自然のヘルム", "自然の力で注入されたヘルム。");
        AddJA(333, "ナキアの木の王冠", "古い木から成長した王冠。");
        AddJA(334, "ナキアの葉の王冠", "森の守護者の伝説的な王冠。");
        AddJA(335, "ナキアの毛皮の鎧", "森の生物で作られた鎧。");
        AddJA(336, "ナキアの先住民の鎧", "森の部族の伝統的な鎧。");
        AddJA(337, "ナキアの緑の木のコート", "生きたつるで織られたコート。");
        AddJA(338, "ナキアの大地のマント", "大地の精霊によって祝福されたマント。");
        AddJA(339, "ナキアの自然の抱擁", "自然と一つになった鎧。");
        AddJA(340, "ナキアの沼の巣の鎧", "深い沼からの鎧。");
        AddJA(341, "ナキアの葉のローブ", "森の守護者の伝説的なローブ。");
        AddJA(342, "ナキアのマンモスの毛皮のショートパンツ", "マンモスの毛皮で作られた温かいショートパンツ。");
        AddJA(343, "ナキアの革のショートパンツ", "伝統的な狩りのすね当て。");
        AddJA(344, "ナキアの雄鹿のすね当て", "雄鹿の皮で作られたすね当て。");
        AddJA(345, "ナキアの大地のすね当て", "大地の精霊によって祝福されたすね当て。");
        AddJA(346, "ナキアの葉のすね当て", "魔法の葉で織られたすね当て。");
        AddJA(347, "ナキアのイノシシ人の腰布", "強力なイノシシ人からの腰布。");
        AddJA(348, "ナキアの血の狩りのすね当て", "血の狩りの伝説的なすね当て。");
        AddJA(349, "毛皮の裏地のブーツ", "温かく快適なブーツ。");
        AddJA(350, "アナグマの皮のブーツ", "アナグマの皮で作られたブーツ。");
        AddJA(351, "コブラのストライクのブーツ", "蛇のように速いブーツ。");
        AddJA(352, "森のストーカーの包帯", "森で静かに動く包帯。");
        AddJA(353, "大地の狩人のブーツ", "獲物を追跡するブーツ。");
        AddJA(354, "熱病の花のレンジャーのブーツ", "熱病の花で飾られたブーツ。彼らは熱狂的な速度と致命的な精度を与える。");
        AddJA(355, "血の捕食者のブーツ", "無数の狩りの血で染まった伝説的なブーツ。ナキアが最高の捕食者になった。");
        
        // Shell Armor (356-383)
        AddJA(356, "シェルの損傷したヘルム", "地下世界からの古いヘルム。");
        AddJA(357, "シェルの壊れたマスク", "まだ保護を提供する壊れたマスク。");
        AddJA(358, "シェルの骨の領主のヘルム", "骨の領主の遺物から鍛造されたヘルム。");
        AddJA(359, "シェルのスケルトンのヘルム", "骨から形成されたヘルム。");
        AddJA(360, "シェルの死のヘルム", "死そのもののヘルム。");
        AddJA(361, "シェルのノクフェラトゥのスケルトンの守護者", "吸血鬼の骨のヘルム。");
        AddJA(362, "シェルの悪魔のヘルム", "悪魔の領主の伝説的なヘルム。");
        AddJA(363, "シェルの葬儀の包帯", "墓からの包帯。");
        AddJA(364, "シェルの古いマント", "古代からの古いマント。");
        AddJA(365, "シェルのノクフェラトゥの骨のマント", "骨で織られたマント。");
        AddJA(366, "シェルの魂のマント", "不安な魂のマント。");
        AddJA(367, "シェルの死のローブ", "死のローブ。");
        AddJA(368, "シェルの地下世界のローブ", "深淵からのローブ。");
        AddJA(369, "シェルの悪魔の鎧", "悪魔の領主の伝説的な鎧。");
        AddJA(370, "シェルの損傷したエプロン", "損傷しているが有用なエプロン。");
        AddJA(371, "シェルの変異した骨のスカート", "変異した骨で作られたスカート。");
        AddJA(372, "シェルのノクフェラトゥの棘の包帯", "棘付きの包帯。");
        AddJA(373, "シェルのノクフェラトゥの肉の守護者", "保存された肉で作られた保護具。");
        AddJA(374, "シェルの血のすね当て", "血で染まったすね当て。");
        AddJA(375, "シェルのノクフェラトゥの血の歩行者", "血の中を歩くすね当て。");
        AddJA(376, "シェルの悪魔のすね当て", "悪魔の領主の伝説的なすね当て。");
        AddJA(377, "装甲のすね当て", "足の下のすべてを粉砕する重い金属のブーツ。防御が最良の攻撃。");
        AddJA(378, "航海者のブーツ", "あらゆる嵐に耐える堅牢なブーツ。彼らはシェルを戦闘の流れに固定する。");
        AddJA(379, "深淵のすね当て", "海の深部で鍛造されたブーツ。彼らはあらゆる圧力に耐える。");
        AddJA(380, "骨粉砕のブーツ", "モンスターの骨で強化されたブーツ。各歩みが脅威。");
        AddJA(381, "マグマの要塞のブーツ", "火山の火で鍛造されたブーツ。彼らは攻撃者を燃やす熱を放つ。");
        AddJA(382, "血の踏みつけのブーツ", "破壊の痕跡を残す残忍なブーツ。敵はそれらと戦うことを恐れる。");
        AddJA(383, "不動の守護者のブーツ", "破壊不可能な防衛者の伝説的なブーツ。シェルが動かない要塞になる。");
        
        // Vesper Armor (384-411)
        AddJA(384, "ヴェスパーの魔女の帽子", "夢の織り手のためのシンプルな帽子。");
        AddJA(385, "ヴェスパーの不気味なフード", "秘密をささやくフード。");
        AddJA(386, "ヴェスパーの奇妙なフード", "奇妙なエネルギーで脈動するフード。");
        AddJA(387, "ヴェスパーの悪夢のエネルギーのフード", "悪夢のエネルギーのフード。");
        AddJA(388, "ヴェスパーの絶望の包帯", "絶望を食べる包帯。");
        AddJA(389, "ヴェスパーの闇の魔術師の王冠", "闇の魔法の王冠。");
        AddJA(390, "ヴェスパーのフェルンブラスの帽子", "闇のマスターの伝説的な帽子。");
        AddJA(391, "ヴェスパーの赤いローブ", "志望の夢の魔術師のためのローブ。");
        AddJA(392, "ヴェスパーの魂の束縛", "魂を導く束縛。");
        AddJA(393, "ヴェスパーのエネルギーのローブ", "エネルギーでパチパチと音を立てるローブ。");
        AddJA(394, "ヴェスパーの幽霊のスカート", "幽霊のエネルギーで織られたスカート。");
        AddJA(395, "ヴェスパーの魂のマント", "魂を含むマント。");
        AddJA(396, "ヴェスパーの魂の包帯", "捕獲された魂の包帯。");
        AddJA(397, "ヴェスパーのアルカナムドラゴンのローブ", "アルカナムドラゴンの伝説的なローブ。");
        AddJA(398, "ヴェスパーの魂のすね当て", "魂のエネルギーで注入されたすね当て。");
        AddJA(399, "ヴェスパーの異国のすね当て", "遠い領域からの異国のすね当て。");
        AddJA(400, "ヴェスパーの魂の脚の鎧", "魂の力を導くすね当て。");
        AddJA(401, "ヴェスパーの血のズボン", "血の魔法で染められたズボン。");
        AddJA(402, "ヴェスパーのマグマのすね当て", "魔法の火で鍛造されたすね当て。");
        AddJA(403, "ヴェスパーの知恵のすね当て", "古代の知恵のすね当て。");
        AddJA(404, "ヴェスパーの古代人のズボン", "古代人からの伝説的なズボン。");
        AddJA(405, "見習いのスリッパ", "神殿の新メンバーが着ける柔らかいスリッパ。彼らは各歩みに祈りを運ぶ。");
        AddJA(406, "神殿の靴", "エルのしもべの伝統的な靴。彼らはヴェスパーを彼女の信仰に固定する。");
        AddJA(407, "奇妙な僧侶のブーツ", "禁止されたテキストを研究する僧侶が着けるブーツ。知識と信仰が絡み合う。");
        AddJA(408, "ノームの祝福の包帯", "ノームの職人によって魔法をかけられた包帯。技術と神性が出会う。");
        AddJA(409, "吸血鬼の絹のスリッパ", "吸血鬼の絹で織られた優雅なスリッパ。彼らは大地そのものから生命を引き出す。");
        AddJA(410, "悪魔殺しのスリッパ", "悪を破壊するために祝福された緑のスリッパ。彼らは各歩みで悪魔を燃やす。");
        AddJA(411, "悪夢の追放者のブーツ", "悪夢を横断して無実の人を救う伝説的なブーツ。ヴェスパーの究極の使命。");
        
        // Yubar Armor (412-439)
        AddJA(412, "ユバルの部族のマスク", "部族の戦士のためのマスク。");
        AddJA(413, "ユバルのバイキングのヘルム", "北の戦士のためのヘルム。");
        AddJA(414, "ユバルの角のヘルム", "恐ろしい角付きのヘルム。");
        AddJA(415, "ユバルの不屈のイクスの頭飾り", "イクスの部族のための頭飾り。");
        AddJA(416, "ユバルの火の恐怖の頭飾り", "恐怖で燃える頭飾り。");
        AddJA(417, "ユバルの不屈のイクスのヘルム", "イクスの戦士のためのヘルム。");
        AddJA(418, "ユバルの黙示録の顔", "黙示録の伝説的な顔。");
        AddJA(419, "ユバルのクマの皮", "強力なクマからの鎧。");
        AddJA(420, "ユバルのマンモスの毛皮のマント", "マンモスからのマント。");
        AddJA(421, "ユバルの不屈のイクスのローブ", "イクスの部族のためのローブ。");
        AddJA(422, "ユバルのマグマのコート", "マグマで鍛造されたコート。");
        AddJA(423, "ユバルの不屈のイクスの胸当て", "イクスの戦士のための胸当て。");
        AddJA(424, "ユバルの溶岩のプレートの鎧", "溶岩のプレートの鎧。");
        AddJA(425, "ユバルの火の巨人の鎧", "火の巨人の伝説的な鎧。");
        AddJA(426, "ユバルの不屈のイクスの腰の鎧", "イクスの部族のための脚の鎧。");
        AddJA(427, "ユバルの変異した皮のズボン", "変異した革で作られたズボン。");
        AddJA(428, "ユバルの不屈のイクスの脚のスカート", "イクスの戦士のための脚のスカート。");
        AddJA(429, "ユバルのドワーフのすね当て", "堅牢なドワーフのすね当て。");
        AddJA(430, "ユバルの合金のすね当て", "合金で強化されたすね当て。");
        AddJA(431, "ユバルのプレートのすね当て", "重いプレートのすね当て。");
        AddJA(432, "ユバルのノームのすね当て", "ノームの工芸の伝説的なすね当て。");
        AddJA(433, "一時的な戦闘ブーツ", "戦場の破片から組み立てられたブーツ。ユバルは見つけたもので間に合わせる。");
        AddJA(434, "部族の包帯", "ユバルの人々の伝統的な包帯。彼らは彼を祖先に結びつける。");
        AddJA(435, "雄鹿の戦士のすね当て", "雄鹿の角で飾られたブーツ。彼らは森の王の力を与える。");
        AddJA(436, "牙の踏みつけのブーツ", "牙のような棘付きの残忍なブーツ。彼らは敵を足の下に踏みつける。");
        AddJA(437, "血のバーサーカーのブーツ", "血のように赤いブーツ、ユバルの怒りを燃やす。痛みが力になる。");
        AddJA(438, "魂の踏みつけ", "倒れた敵の魂を収穫するブーツ。彼らの力が各戦闘で成長する。");
        AddJA(439, "帰郷のチャンピオンのブーツ", "精神的にユバルを故郷に連れ戻す伝説的なブーツ。彼の祖先が彼と並んで戦う。");
        
        // Legendary Items (440-443)
        AddJA(440, "夢の歩行者の翼のヘルム", "夢の歩行者に飛行能力を与える伝説的なヘルム。最も近い敵を自動的に攻撃する。");
        AddJA(441, "夢の魔法のプレートの鎧", "すべての領域の夢の歩行者を保護する伝説的な鎧。最も近い敵を自動的に狙う。");
        AddJA(442, "深淵のすね当て", "夢の最も深い部分で鍛造された伝説的なすね当て。");
        AddJA(443, "水上歩行のブーツ", "着用者が水上を歩くことを可能にする伝説的なブーツ。すべての領域の夢の歩行者が探している宝物。");
        
        // Consumables (444-447)
        AddJA(444, "夢見る者のトニック", "目覚めの世界で見つけたハーブから作られたシンプルな治療薬。最も淡い夢でさえ、一滴の希望から始まる。");
        AddJA(445, "夢想のエッセンス", "穏やかな眠りの記憶から蒸留された薬。飲む者は忘れられた夢の温かさが傷を癒すのを感じる。");
        AddJA(446, "明晰なる生命力", "スムージーが星屑と悪夢の欠片を使って作り出した。液体は千の眠る星の光で輝く。");
        AddJA(447, "エルの霊薬", "エル自身の光に祝福された聖なる飲み物。ヴェスパーの騎士団はそのレシピを厳重に守る。最も暗い悪夢によって与えられた傷さえも癒せるからだ。");
        
        // Universal Legendary Belt (448)
        AddJA(448, "無限の夢のベルト", "装着者を永遠の夢のサイクルに結びつける伝説的なベルト。悪夢を征服するたびに力が増す。");
        
        // Hero-Specific Legendary Belts (449-457)
        AddJA(449, "オウレナの生命織者のベルト", "オウレナの盗まれた生命力研究から鍛造されたベルト。生命と死を完璧なバランスに織り込む。");
        AddJA(450, "ビスマスの結晶の帯", "盲目の少女の視覚と共に脈動する純粋な結晶のベルト。目が見えないものを見ることができる。");
        AddJA(451, "ラセルタの深紅の眼のベルト", "ラセルタの伝説的な視線を導くベルト。その警戒の力から逃れる標的はない。");
        AddJA(452, "ミストの決闘者チャンピオンのベルト", "アストリッド家の究極のベルト。ミストの名誉がすべての糸に織り込まれている。");
        AddJA(453, "ナチアの世界樹のベルト", "世界樹そのものから育ったベルト。自然の怒りがその中を流れる。");
        AddJA(454, "ハスクの破壊不能の鎖", "破壊不能な決意から鍛造されたベルト。ハスクの決意がその力である。");
        AddJA(455, "シェルの影の暗殺者のベルト", "完璧な闇のベルト。シェルの操縦者はその意志を破ることができなかった。");
        AddJA(456, "ヴェスパーの黄昏の審問官のベルト", "エルの騎士団の最高位のベルト。悪夢を裁き、闇を追放する。");
        AddJA(457, "ユバルの祖先チャンピオンのベルト", "ユバルの祖先を呼び出すベルト。彼らの力がすべての繊維を通って流れる。");
        
        // Universal Legendary Ring (458)
        AddJA(458, "永遠の夢の指輪", "装着者を無限の夢の領域に接続する伝説的な指輪。その力はすべての境界を超越する。");
        
        // Hero-Specific Legendary Rings (459-467)
        AddJA(459, "オウレナの大賢者の指輪", "月のアルカナムの大賢者の指輪。オウレナは彼らの偽善の証としてそれを取った。");
        AddJA(460, "ビスマスの宝石の心の指輪", "純粋な結晶魔法の指輪。盲目の少女の変容した本質と共に脈動する。");
        AddJA(461, "ラセルタの王室の処刑人の指輪", "王室警備隊のエリート処刑人の指輪。ラセルタは無数の戦いを通じてそれを獲得した。");
        AddJA(462, "ミストのアストリッドの遺産の指輪", "アストリッド家の祖先の指輪。ミストの血統がその金属を通って流れる。");
        AddJA(463, "ナチアの森の守護者の指輪", "ナチアの森の精霊に祝福された指輪。自然の力がその本質である。");
        AddJA(464, "ハスクの不屈の決意の指輪", "破壊不能な意志から鍛造された指輪。ハスクの決意がその力である。");
        AddJA(465, "シェルの完璧な影の指輪", "絶対的な闇の指輪。シェルの操縦者はその力を制御することができなかった。");
        AddJA(466, "ヴェスパーの聖なる炎の指輪", "エルの聖なる炎の純粋な本質を含む指輪。すべての闇を追放する。");
        AddJA(467, "ユバルの部族チャンピオンの指輪", "ユバルの祖先の力を導く指輪。彼らの怒りがすべての打撃を強化する。");
        
        // Universal Legendary Amulet (468)
        AddJA(468, "無限の夢の護符", "装着者を永遠の夢のサイクルに結びつける伝説的な護符。その力はすべての境界を超越する。");
        
        // Hero-Specific Legendary Amulets (469-477)
        AddJA(469, "オウレナの生命織者の護符", "オウレナの盗まれた生命力研究から鍛造された護符。生命と死を完璧なバランスに織り込む。");
        AddJA(470, "ビスマスの結晶の心", "盲目の少女の視覚と共に脈動する純粋な結晶の護符。目が見えないものを見ることができる。");
        AddJA(471, "ラセルタの深紅の眼の護符", "ラセルタの伝説的な視線を導く護符。その警戒の力から逃れる標的はない。");
        AddJA(472, "ミストの決闘者チャンピオンの護符", "アストリッド家の究極の護符。ミストの名誉がすべての宝石に織り込まれている。");
        AddJA(473, "ナチアの世界樹の護符", "世界樹そのものから育った護符。自然の怒りがその中を流れる。");
        AddJA(474, "ハスクの破壊不能の護符", "破壊不能な決意から鍛造された護符。ハスクの決意がその力である。");
        AddJA(475, "シェルの影の暗殺者の護符", "完璧な闇の護符。シェルの操縦者はその意志を破ることができなかった。");
        AddJA(476, "ヴェスパーの黄昏の審問官の護符", "エルの騎士団の最高位の護符。悪夢を裁き、闇を追放する。");
        AddJA(477, "ユバルの祖先チャンピオンの護符", "ユバルの祖先を呼び出す護符。彼らの力がすべての宝石を通って流れる。");
    }
    
    private static void AddJA(int id, string name, string desc)
    {
        _nameJA[id] = name;
        _descJA[id] = desc;
    }
    
    // ============================================================
    // KOREAN TRANSLATIONS
    // ============================================================
    private static void InitializeKO()
    {
        _nameKO = new Dictionary<int, string>();
        _descKO = new Dictionary<int, string>();
        
        // Aurena Weapons (1-18)
        AddKO(1, "초보자의 발톱", "월화 오컬트 협회의 신입 회원에게 주어진 단순한 발톱. 아우레나는 추방 후에도 자신의 것을 보관했다.");
        AddKO(2, "학자의 발톱", "금지된 텍스트에 갈아낸 발톱. 그것이 남긴 상처는 비밀을 속삭인다.");
        AddKO(3, "달빛의 발톱", "달빛 아래에서 약하게 빛나는 발톱. 그것들은 달의 에너지를 파괴적인 공격으로 전환한다.");
        AddKO(4, "오컬트 면도날", "치유 룬이 새겨진 발톱 - 이제 치유 대신 찢는 데 사용된다.");
        AddKO(5, "추방자의 발톱", "추방 후 비밀리에 단조된 발톱. 매번 베임은 협회가 그녀에게서 빼앗은 것을 상기시킨다.");
        AddKO(6, "이단자의 손아귀", "다른 추방된 현자에게 속했던 발톱. 그들의 영혼이 아우레나의 공격을 인도한다.");
        AddKO(7, "희생의 발톱", "아우레나의 생명력이 쇠약해질수록 더 날카로워지는 발톱. 협회는 이 연구가 너무 위험하다고 판단했다.");
        AddKO(8, "서리의 발톱", "협회의 가장 깊은 금고의 얼음으로 얼린 발톱. 그것들의 접촉은 육체와 영혼을 마비시킨다.");
        AddKO(9, "달의 찢는 자", "금지된 마법으로 강화됨. 그것들이 입힌 모든 상처는 아우레나의 동맹을 치유한다.");
        AddKO(10, "빛나는 발톱", "도둑맞은 햇빛으로 축복받은 발톱. 아우레나의 빛 마법 연구는 태양과 달의 두 종교를 분노하게 했다.");
        AddKO(11, "잿불의 발톱", "자원한 꿈꾸는 자의 생명력으로 점화된 발톱. 불꽃은 절대 꺼지지 않는다.");
        AddKO(12, "그림자의 발톱", "악몽의 정수에 담근 발톱. 아우레나는 어둠이 빛이 치유할 수 없는 것을 치유한다는 것을 배웠다.");
        AddKO(13, "빙하의 수확자", "영원한 얼음으로 조각된 발톱. 그것들은 적의 피를 얼리고 아우레나의 생명력을 보존한다.");
        AddKO(14, "불사조의 발톱", "불사조의 불꽃에서 단조되고 아우레나 자신의 피로 담금질된 발톱. 그것들은 꺼지지 않는 분노로 타오른다.");
        AddKO(15, "공허의 찢는 자", "현실 자체를 찢을 수 있는 발톱. 협회는 아우레나가 다음에 무엇을 발견할지 두려워한다.");
        AddKO(16, "천상의 발톱", "순수한 별빛에 스며든 발톱. 매 타격은 신들이 숨기고 싶어하는 지식에 대한 기도이다.");
        AddKO(17, "대현자의 발톱", "협회의 대현자의 의식용 발톱. 아우레나는 그것들을 그들의 위선의 증거로 사용한다.");
        AddKO(18, "생명의 직조자의 발톱", "아우레나의 걸작 - 생명력을 파괴하지 않고 훔칠 수 있는 발톱. 협회는 이것을 이단이라고 부른다.");
        
        // Bismuth Weapons (19-36)
        AddKO(19, "빈 그리무아르", "채워지기를 기다리는 빈 주문서. 비스무트 자신처럼, 그 페이지는 무한한 잠재력을 포함한다.");
        AddKO(20, "견습생의 책", "초보자를 위한 주문서. 비스무트가 된 맹인 소녀는 손가락으로 그 페이지를 따라가며 마법을 꿈꿨다.");
        AddKO(21, "수정의 코덱스", "페이지가 얇은 보석으로 만들어진 책. 비스무트의 수정 본성과 공명한다.");
        AddKO(22, "방랑자의 일기", "보지 못했지만 항상 알고 있던 맹인 소녀의 관찰을 기록하는 여행 일기.");
        AddKO(23, "보석으로의 입문", "수정 마법의 기본 가이드. 그 단어는 비스무트의 무지개색 몸처럼 반짝인다.");
        AddKO(24, "맹인의 고전", "맹인을 위해 볼록한 글자로 쓰여진 책. 비스무트는 촉각과 기억으로 그것을 읽는다.");
        AddKO(25, "잿불의 말의 책", "페이지가 영원히 타는 책. 불꽃은 비스무트가 느끼지만 보지 못하는 색으로 말한다.");
        AddKO(26, "얼음의 사전", "영원한 얼음에 싸인 책. 그 얼음 페이지는 꿈이 시작되기 전의 지식을 보존한다.");
        AddKO(27, "빛나는 필사본", "내부의 빛을 발산하는 책. 비스무트가 되기 전에, 그것은 맹인 소녀를 어둠을 통해 인도했다.");
        AddKO(28, "그림자의 그리무아르", "빛을 흡수하는 책. 그 어두운 페이지는 비스무트조차 읽기를 두려워하는 비밀을 포함한다.");
        AddKO(29, "살아있는 주문서", "맹인 소녀와의 융합을 선택한 의식적인 책. 함께 그들은 더 큰 것이 되었다.");
        AddKO(30, "프리즘의 코덱스", "마법을 구성 색상으로 분해하는 책. 비스무트는 그것이 굴절하는 빛을 통해 세계를 본다.");
        AddKO(31, "공허의 연대기", "어둠에서 잃어버린 자의 기억을 기록하는 책. 비스무트는 그들을 기리기 위해 그들의 이야기를 읽는다.");
        AddKO(32, "보석의 심장 그리무아르", "맹인 소녀와 융합한 원래의 주문서. 그 페이지는 수정의 생명으로 맥동한다.");
        AddKO(33, "눈부신 빛의 책", "보는 자를 눈멀게 할 정도로 눈부신 책. 비스무트에게는 단순히 따뜻함일 뿐이다.");
        AddKO(34, "서리에 묶인 연대기", "시간에 얼어붙은 고대의 책. 그 예언은 소녀처럼 걷는 보석에 대해 이야기한다.");
        AddKO(35, "이름 없는 책", "이름 없는 책, 시력 없는 소녀처럼. 함께 그들은 자신의 정체성을 찾았다.");
        AddKO(36, "지옥의 고전", "맹인이 자신을 정의하는 것을 거부하는 꿈꾸는 자의 열정으로 타는 책.");
        
        // Lacerta Weapons (37-54)
        AddKO(37, "신병의 소총", "왕실 근위대 신병에게 발급되는 표준 무기. 라세르타는 훈련 첫 주가 끝나기 전에 그것을 마스터했다.");
        AddKO(38, "사냥꾼의 장총", "신뢰할 수 있는 사냥용 소총. 라세르타는 근위대에 합류하기 전에 가족을 부양하기 위해 비슷한 총을 사용했다.");
        AddKO(39, "순찰 카빈총", "컴팩트하고 신뢰할 수 있다. 왕국 국경에서의 긴 순찰에 완벽하다.");
        AddKO(40, "저격수의 총", "위력보다 정확도를 중시하는 자를 위해 설계된 균형 잡힌 소총.");
        AddKO(41, "흑색 화약 머스킷", "오래되었지만 신뢰할 수 있다. 흑색 화약의 냄새가 라세르타에게 그의 첫 사냥을 상기시킨다.");
        AddKO(42, "정찰 연발총", "정찰 임무를 위한 경량 소총. 속도와 은밀성이 원시적인 위력을 능가한다.");
        AddKO(43, "왕실 근위대 소총", "라세르타가 복무 중에 휴대한 무기. 각 흠집이 이야기를 말한다.");
        AddKO(44, "저격수의 자존심", "결코 놓치지 않는 자를 위한 정밀 소총. 라세르타의 주홍색 눈은 다른 자가 볼 수 없는 것을 본다.");
        AddKO(45, "타오르는 장총", "화염 탄두를 장전. 적은 총알이 명중한 후에도 오래 타오른다.");
        AddKO(46, "서리 물기 카빈총", "절대 영도까지 냉각된 총알을 발사. 목표는 치명적인 일격 전에 달팽이처럼 느려진다.");
        AddKO(47, "밤의 소총", "어둠에서 사냥하기 위한 무기. 그 발사는 그림자처럼 조용하다.");
        AddKO(48, "새벽의 머스킷", "새벽의 빛으로 축복받음. 그 발사는 어둠과 속임수를 관통한다.");
        AddKO(49, "주홍색 눈", "라세르타의 전설적인 시선에 따라 명명됨. 이 소총의 시야에서 벗어나는 목표는 없다.");
        AddKO(50, "왕실 처형자", "근위대 엘리트를 위해 예약됨. 라세르타는 왕국을 구한 후 이 소총을 받았다.");
        AddKO(51, "태양의 대포", "농축된 빛을 발사하는 소총. 각 발사는 미니어처 일출이다.");
        AddKO(52, "공허의 사냥꾼", "순수한 어둠의 총알을 발사하는 소총. 목표는 흔적 없이 사라진다.");
        AddKO(53, "절대 영도", "존재에서 가장 추운 지점. 시간조차 그것 앞에서 얼어붙는다.");
        AddKO(54, "용의 숨결", "폭발성 화염 탄을 장전한 소총. 왕국의 적은 그 포효를 두려워하는 것을 배웠다.");
        
        // Mist Weapons (55-70)
        AddKO(55, "훈련용 레이피어", "미스트의 젊은 시절 훈련용 검. 그때도 그녀는 아스트리드의 모든 교관을 능가했다.");
        AddKO(56, "귀족의 레이피어", "미스트의 귀족 혈통에 맞는 레이피어. 가볍고 우아하며 치명적이다.");
        AddKO(57, "결투자의 레이피어", "일대일 전투를 위해 설계된 레이피어. 미스트는 결투에서 한 번도 패배하지 않았다.");
        AddKO(58, "신속한 레이피어", "미스트의 팔의 연장처럼 가볍다.");
        AddKO(59, "아스트리드의 레이피어", "아스트리드의 최고의 대장간에서 단조됨. 왕국 공예의 상징.");
        AddKO(60, "댄서의 레이피어", "전투를 예술로 취급하는 자를 위한 레이피어. 미스트의 움직임은 시적이다.");
        AddKO(61, "타오르는 레이피어", "불꽃에 둘러싸인 레이피어. 미스트의 분노는 그녀의 결심만큼 뜨겁다.");
        AddKO(62, "그림자의 레이피어", "어둠에서 타격하는 레이피어. 적은 미스트가 움직이는 것을 보기 전에 쓰러진다.");
        AddKO(63, "서리 물기 레이피어", "얼어붙은 강철의 레이피어. 그 접촉은 몸과 영혼을 마비시킨다.");
        AddKO(64, "카밀라의 은총", "미스트가 소중히 여긴 자로부터의 선물. 그녀는 그들의 기억을 기리기 위해 싸운다.");
        AddKO(65, "막는 레이피어", "방어와 공격을 위해 설계된 레이피어. 미스트는 각 공격을 기회로 전환한다.");
        AddKO(66, "빛나는 레이피어", "내부의 빛을 발산하는 레이피어. 그것은 거짓말과 그림자를 관통한다.");
        AddKO(67, "지옥의 레이피어", "꺼지지 않는 열정으로 타는 레이피어. 미스트의 결심은 꺼뜨릴 수 없다.");
        AddKO(68, "한밤중의 레이피어", "절대적인 어둠에서 단조된 레이피어. 그것은 죽음의 침묵으로 타격한다.");
        AddKO(69, "겨울의 레이피어", "영원한 얼음에서 조각된 레이피어. 그 추위는 전투에서 미스트의 집중력에만 필적한다.");
        AddKO(70, "불굴의 레이피어", "아스트리드의 최고의 결투자의 전설적인 레이피어. 미스트는 무수한 승리로 이 칭호를 획득했다.");
        
        // Nachia Weapons (71-88)
        AddKO(71, "새싹 지팡이", "영혼의 숲에서 온 젊은 가지. 새싹조차 나키아의 부름에 응답한다.");
        AddKO(72, "영혼 소환자", "숲의 영혼의 목소리를 인도하도록 조각된 지팡이. 그것들은 나키아에게 비밀을 속삭인다.");
        AddKO(73, "숲의 가지", "여전히 자라고 있는 살아있는 가지. 숲의 마법이 그 안을 흐른다.");
        AddKO(74, "야생의 지팡이", "나키아 자신처럼 야생적이고 예측 불가능하다. 영혼들은 그 혼돈의 에너지를 사랑한다.");
        AddKO(75, "수호자의 지팡이", "숲을 보호하는 자가 휴대한다. 나키아는 이 책임을 가지고 태어났다.");
        AddKO(76, "영혼계의 지팡이", "물질 세계와 영혼 세계를 연결하는 지팡이. 나키아는 둘 사이를 걷는다.");
        AddKO(77, "서리 나무 지팡이", "영원한 겨울에 얼어붙은 지팡이. 북쪽의 차가운 영혼들이 그 부름에 응답한다.");
        AddKO(78, "그림자 뿌리 지팡이", "숲의 가장 깊은 부분에서 자란다. 그림자 영혼들이 그 주위에서 춤춘다.");
        AddKO(79, "잿불 나무 지팡이", "타는 것을 결코 멈추지 않는 지팡이. 불의 영혼들이 그 따뜻함에 끌린다.");
        AddKO(80, "태양의 축복 지팡이", "새벽의 영혼들로 축복받음. 그 빛은 길 잃은 영혼들을 집으로 인도한다.");
        AddKO(81, "펜리르의 송곳니", "꼭대기에 영혼 늑대의 송곳니가 박힌 지팡이. 나키아의 충실한 동반자가 그 힘을 인도한다.");
        AddKO(82, "고목 지팡이", "천년 된 참나무에서 조각됨. 가장 오래된 영혼들은 그것이 심어졌을 때를 기억한다.");
        AddKO(83, "세계수의 가지", "전설의 세계수에서 온 가지. 모든 숲의 영혼들이 그것에게 절한다.");
        AddKO(84, "영혼왕의 지팡이", "영혼왕 자신의 지팡이. 나키아는 숲을 보호함으로써 그것을 획득했다.");
        AddKO(85, "영원한 서리 지팡이", "숲의 얼어붙은 심장에서 온 영원한 얼음의 지팡이. 겨울 영혼들이 그 명령에 복종한다.");
        AddKO(86, "불사조의 보금자리", "불의 영혼들이 둥지를 틀 지팡이. 그 불꽃은 파괴가 아닌 재생을 가져온다.");
        AddKO(87, "빛나는 숲 지팡이", "천 마리의 반딧불의 빛을 발산하는 지팡이. 희망의 영혼들이 그 안에서 춤춘다.");
        AddKO(88, "공허의 수호자 지팡이", "세계의 경계를 지키는 지팡이. 그림자 영혼들이 그 소유자를 보호한다.");
        
        // Shell Weapons (89-106)
        AddKO(89, "초보자의 칼", "검의 길을 배우는 자를 위한 기본 사무라이 칼. 쉘은 며칠 만에 그것을 마스터했다.");
        AddKO(90, "침묵의 칼", "소리를 내지 않도록 설계된 사무라이 칼. 쉘의 희생자들은 죽음이 다가오는 것을 결코 듣지 못한다.");
        AddKO(91, "암살자의 칼", "가늘고 정확하다. 쉘은 올바른 위치에 한 번의 타격만 필요하다.");
        AddKO(92, "살인자의 칼", "효율적이다. 실용적이다. 무자비하다. 쉘 자신처럼.");
        AddKO(93, "사냥꾼의 칼", "사냥을 위한 사무라이 칼. 쉘은 목표가 제거될 때까지 멈추지 않는다.");
        AddKO(94, "공허의 칼", "쉘이 느끼는 공허에 따라 명명됨. 또는 느끼지 못하는.");
        AddKO(95, "처형자의 칼", "무수한 생명을 끝내는 사무라이 칼. 쉘은 그 중 어느 것에도 아무것도 느끼지 않는다.");
        AddKO(96, "정밀의 칼", "약점을 이용하도록 조정된 사무라이 칼. 쉘은 최적의 공격을 계산한다.");
        AddKO(97, "서리 물기 칼", "차가운 악의의 사무라이 칼. 그 추위는 쉘의 공허한 마음과 어울린다.");
        AddKO(98, "정화의 칼", "어두운 일을 위한 밝은 사무라이 칼. 쉘은 아이러니를 느끼지 않는다.");
        AddKO(99, "그림자의 칼", "그림자를 마시는 사무라이 칼. 쉘은 보이지 않게 움직이고 소리 없이 타격한다.");
        AddKO(100, "타오르는 단검", "내부의 불로 가열된 사무라이 칼. 쉘의 목표는 피를 흘리기 전에 타오른다.");
        AddKO(101, "완벽한 살인자의 도구", "궁극의 살인자의 궁극의 도구. 쉘은 이 칼을 위해 태어났다.");
        AddKO(102, "무자비한 칼날", "결코 무뎌지지 않는 사무라이 칼, 결코 멈추지 않는 추적자에 의해 휘둘러짐.");
        AddKO(103, "지옥의 암살자", "지옥의 불꽃에서 단조된 사무라이 칼. 그것은 쉘의 유일한 목적의 강도로 타오른다.");
        AddKO(104, "공허의 칼", "절대적인 어둠의 사무라이 칼. 쉘처럼, 그것은 끝내기 위해서만 존재한다.");
        AddKO(105, "얼어붙은 칼", "영원한 얼음의 사무라이 칼. 그 판결은 쉘만큼 차갑고 최종적이다.");
        AddKO(106, "빛나는 종결", "밝은 사무라이 칼의 궁극 형태. 쉘은 엘의 이름으로 판결을 집행한다.");
        
        // Vesper Weapons (107-124)
        AddKO(107, "초보자의 홀", "태양의 불꽃 기사단의 신입 회원을 위한 단순한 홀. 베스퍼는 여기서 그의 여정을 시작했다.");
        AddKO(108, "심판의 망치", "엘의 의지를 집행하는 데 사용되는 전투 망치. 각 타격은 신성한 판결이다.");
        AddKO(109, "불의 수호자의 막대", "성스러운 불을 지키는 자가 휴대하는 무기. 그 타격은 불결함을 정화한다.");
        AddKO(110, "수습생의 방패", "기사단 수습생을 위한 단순한 방패. 신앙이 최고의 방어다.");
        AddKO(111, "종교재판관의 방패", "종교재판관이 휴대하는 방패. 그것은 무수한 이단자의 몰락을 목격했다.");
        AddKO(112, "황혼의 방어자", "빛과 어둠 사이를 지키는 방패.");
        AddKO(113, "광신도의 분쇄 망치", "종교적 광신으로 타는 전투 망치. 적은 그것 앞에서 떤다.");
        AddKO(114, "정화자의 무거운 망치", "악을 정화하는 데 사용되는 거대한 전투 망치. 한 번의 타격으로 죄를 분쇄한다.");
        AddKO(115, "엘의 심판", "엘 자신에 의해 개인적으로 축복받은 전투 망치. 그 판결은 항소할 수 없다.");
        AddKO(116, "신앙의 요새", "파괴 불가능한 방패. 신앙이 꺼지지 않는 한, 그것은 부서지지 않는다.");
        AddKO(117, "성스러운 불꽃 방패", "성스러운 불꽃에 둘러싸인 방패. 그것을 만지는 악은 불타버린다.");
        AddKO(118, "종교재판의 요새", "고위 종교재판관의 방패. 베스퍼는 무자비한 봉사로 그것을 획득했다.");
        AddKO(119, "새벽의 분쇄자", "어둠을 분쇄하는 전설적인 전투 망치. 베스퍼는 엘의 분노의 화신을 휘두른다.");
        AddKO(120, "신성한 파괴자", "최고 사제에 의해 축복받은 전투 망치. 그 타격은 신성한 분노의 무게를 운반한다.");
        AddKO(121, "엘의 오른손", "기사단의 가장 신성한 무기. 베스퍼는 엘의 의지의 선택된 도구다.");
        AddKO(122, "불꽃의 요새", "기사단의 궁극 방어. 엘의 빛이 베스퍼를 모든 해로부터 보호한다.");
        AddKO(123, "영원한 경계", "흔들리지 않는 방패. 베스퍼처럼, 그것은 항상 어둠을 지켜본다.");
        AddKO(124, "황혼 성기사의 맹세", "기사단 지도자의 방패. 베스퍼는 영혼을 엘에 묶겠다고 맹세했다.");
        
        // Yubar Weapons (125-142)
        AddKO(125, "새로 태어난 별", "방금 형성된 별. 유바르는 모든 별이 그렇게 작았을 때를 기억한다.");
        AddKO(126, "우주의 잿불", "별의 불꽃의 파편. 그것은 창조 자체의 열로 타오른다.");
        AddKO(127, "별가루 구", "형성되기를 기다리는 압축된 별가루. 유바르는 그런 물질로 은하를 짠다.");
        AddKO(128, "꿈의 촉매", "꿈의 에너지를 증폭시키는 구. 유바르의 첫 실험에서 유래.");
        AddKO(129, "성운의 핵심", "먼 성운의 심장. 유바르는 우주의 직물에서 그것을 따냈다.");
        AddKO(130, "천체의 씨앗", "별이 될 씨앗. 유바르는 무수한 그런 씨앗을 기른다.");
        AddKO(131, "초신성의 파편", "폭발한 별의 파편. 유바르는 그 죽음을 목격하고 그 힘을 보존했다.");
        AddKO(132, "중력의 우물", "중력을 압축하는 구. 공간 자체가 그 주위에서 구부러진다.");
        AddKO(133, "공허의 특이점", "무한한 밀도의 지점. 유바르는 그것을 사용하여 세계를 창조하고 파괴한다.");
        AddKO(134, "태양의 코로나", "태양의 외부 대기의 파편. 그것은 별의 분노로 타오른다.");
        AddKO(135, "얼어붙은 혜성", "포획된 혜성의 핵심. 그것은 우주의 깊이에서 온 비밀을 운반한다.");
        AddKO(136, "별의 용광로", "별의 핵심의 축소판. 유바르는 그것을 사용하여 새로운 현실을 단조한다.");
        AddKO(137, "빅뱅의 유물", "창조의 빅뱅의 파편. 그것은 우주의 기원을 포함한다.");
        AddKO(138, "우주의 직조기", "현실을 짜는 도구. 유바르는 그것을 사용하여 운명을 재형성한다.");
        AddKO(139, "빛나는 창조", "창조 자체의 빛. 유바르는 그것을 사용하여 첫 번째 별들을 탄생시켰다.");
        AddKO(140, "엔트로피의 심장", "우주 붕괴의 정수. 모든 것이 끝나고, 유바르가 언제를 결정한다.");
        AddKO(141, "절대 영도", "존재에서 가장 추운 지점. 시간조차 그것 앞에서 얼어붙는다.");
        AddKO(142, "재앙의 핵심", "우주 파괴의 심장. 유바르는 위기의 순간에만 그것을 해제한다.");
        
        // Rings (143-162)
        AddKO(143, "꿈꾸는 자의 반지", "꿈의 세계에 막 들어가는 자가 착용하는 단순한 반지. 그것은 약한 희망을 발산한다.");
        AddKO(144, "별가루 반지", "응축된 별가루에서 단조됨. 꿈꾸는 자들은 위험한 때에 그것이 더 밝게 빛난다고 말한다.");
        AddKO(145, "잠의 표시", "평화로운 잠의 표시가 새겨져 있다. 착용자는 상처에서 더 빨리 회복한다.");
        AddKO(146, "영혼의 반지", "떠도는 영혼을 포착하는 반지. 그들의 빛이 어두운 꿈을 통과하는 길을 인도한다.");
        AddKO(147, "방랑자의 반지", "꿈의 영역의 방랑자가 착용한다. 그것은 무수한 잊혀진 길을 목격했다.");
        AddKO(148, "악몽 파편 반지", "패배한 악몽의 파편을 포함하는 반지. 그 어둠이 용감한 자를 강화한다.");
        AddKO(149, "기억의 수호자", "소중한 기억을 보존하는 반지. 스무디는 자신의 가게에서 비슷한 것을 판다.");
        AddKO(150, "달의 오컬트 반지", "달의 오컬트 협회에서 온 반지. 아우레나는 추방 전에 비슷한 것을 착용했다.");
        AddKO(151, "영혼의 숲 표시", "나키아의 숲의 영혼들로 축복받은 반지. 자연의 힘이 그 안을 흐른다.");
        AddKO(152, "왕실 근위대 배지", "라세르타가 복무 중에 착용한 반지. 그것은 여전히 책임의 무게를 운반한다.");
        AddKO(153, "결투자의 영광", "귀족 결투자들 사이에서 전해지는 반지. 미스트의 가족은 그런 반지를 소중히 여긴다.");
        AddKO(154, "성스러운 불꽃 잿불 부적", "봉인된 성스러운 불꽃의 불꽃. 베스퍼의 기사단이 이 유물들을 지킨다.");
        AddKO(155, "주문서 파편", "비스무트의 수정 마법으로 만든 반지. 그것은 오컬트 에너지로 윙윙거린다.");
        AddKO(156, "사냥꾼의 표시", "사냥감을 추적하는 반지. 쉘의 조종자들은 그것들을 사용하여 자신의 도구를 감시한다.");
        AddKO(157, "엘의 축복", "엘 자신에 의해 축복받은 반지. 그 빛이 악몽을 쫓고 부상자를 치유한다.");
        AddKO(158, "악몽 군주의 표시", "패배한 악몽 군주로부터 취함. 그 어둠의 힘이 타락시키고 강화한다.");
        AddKO(159, "아스트리드의 유산", "아스트리드의 귀족 가문에서 온 반지. 미스트의 조상들은 무수한 결투에서 그것을 착용했다.");
        AddKO(160, "꿈꾸는 자의 눈", "꿈의 베일을 통해 볼 수 있는 반지. 과거, 현재, 미래가 섞인다.");
        AddKO(161, "원시 꿈의 반지", "꿈의 새벽에 단조됨. 그것은 첫 번째 꿈꾸는 자의 정수를 포함한다.");
        AddKO(162, "영원한 잠", "가장 깊은 잠에 닿는 반지. 착용자는 죽음도 깨어남도 두려워하지 않는다.");
        
        // Amulets (163-183)
        AddKO(163, "꿈꾸는 자의 펜던트", "꿈의 세계에 막 들어가는 자가 착용하는 단순한 펜던트. 그것은 약한 희망을 발산한다.");
        AddKO(164, "불의 접촉 펜던트", "불에 닿은 펜던트. 그 따뜻함이 추위와 공포를 쫓는다.");
        AddKO(165, "주홍색 눈물", "피 눈물 모양의 루비. 그 힘은 희생에서 온다.");
        AddKO(166, "호박 부적", "고대 생물을 포함하는 호박. 그 기억이 착용자를 강화한다.");
        AddKO(167, "황혼의 메달", "빛과 어둠 사이에서 깜빡이는 메달. 베스퍼의 신입 회원들이 이것들을 착용한다.");
        AddKO(168, "별가루 목걸이", "수정 별들로 뿌려진 목걸이. 스무디는 떨어진 꿈에서 그것들을 수집한다.");
        AddKO(169, "에메랄드의 눈", "환상을 통해 볼 수 있는 녹색 보석. 악몽은 그 시선에서 벗어날 수 없다.");
        AddKO(170, "숲의 영혼의 심장", "숲의 영혼들의 축복을 포함하는 보석. 나키아의 수호자들이 이것들을 소중히 여긴다.");
        AddKO(171, "달의 오컬트 표시", "금지된 아카이브에서 온 표시. 아우레나는 그것들을 연구하기 위해 모든 것을 위험에 빠뜨린다.");
        AddKO(172, "비스무트의 수정", "수정의 순수한 마법의 파편. 그것은 오컬트 에너지와 공명한다.");
        AddKO(173, "성스러운 불꽃 잿불 부적", "봉인된 성스러운 불꽃의 불꽃. 베스퍼의 기사단이 이 유물들을 지킨다.");
        AddKO(174, "악몽의 송곳니 펜던트", "강력한 악몽에서 온 송곳니, 이제는 트로피. 쉘은 기억으로 하나를 착용한다.");
        AddKO(175, "왕실 근위대 메달", "왕실 근위대에서 온 명예의 메달. 라세르타는 추방 전에 많은 것을 획득했다.");
        AddKO(176, "아스트리드의 문장", "아스트리드 가족의 귀족 문장. 미스트의 가족 유산이 이 사슬에 걸려 있다.");
        AddKO(177, "엘의 성스러운 눈물", "엘 자신에 의해 흘려진 눈물. 그 빛이 가장 어두운 악몽으로부터 보호한다.");
        AddKO(178, "원시 에메랄드", "첫 번째 숲에서 온 보석. 그것은 고대 영혼들의 꿈을 포함한다.");
        AddKO(179, "악몽 군주의 눈", "패배한 악몽 군주의 눈. 그것은 모든 공포를 보고 이용한다.");
        AddKO(180, "불꽃의 심장", "성스러운 불꽃 자체의 핵심. 가장 경건한 자만이 그것을 착용할 수 있다.");
        AddKO(181, "꿈꾸는 자의 신탁", "모든 가능한 미래를 볼 수 있는 부적. 현실이 그 예언 앞에서 구부러진다.");
        AddKO(182, "공허의 방랑자 나침반", "공허를 가리키는 부적. 그것을 따르는 자는 결코 같은 방법으로 돌아오지 않는다.");
        AddKO(183, "미다스의 부적", "금의 꿈에 닿은 전설적인 부적. 패배한 적은 추가 금을 떨어뜨린다.");
        AddKO(478, "별가루 수집가", "패배한 적으로부터 결정화된 별가루를 수집하는 전설적인 부적. 각 처치마다 소중한 꿈의 정수를 생성한다 (기본 1-5, 강화 레벨과 몬스터 강도에 따라 스케일).");
        
        // Belts (184-203)
        AddKO(184, "방랑자의 벨트", "꿈 사이를 떠도는 자가 착용하는 단순한 천 벨트. 그것은 영혼을 고정한다.");
        AddKO(185, "꿈꾸는 자의 밧줄", "꿈의 실로 짠 밧줄. 그것은 각 심장 박동으로 부드럽게 맥동한다.");
        AddKO(186, "별가루 버클", "수정 별가루로 장식된 벨트. 스무디는 비슷한 액세서리를 판다.");
        AddKO(187, "보초의 벨트", "꿈의 수호자가 착용하는 견고한 벨트. 그것은 결코 느슨해지지 않고 결코 실패하지 않는다.");
        AddKO(188, "신비로운 벨트", "약한 마법으로 주입된 벨트. 악몽이 가까워지면 찌른다.");
        AddKO(189, "사냥꾼의 벨트", "공급 가방이 있는 가죽 벨트. 꿈 속에서의 긴 여행에 필수적이다.");
        AddKO(190, "순례자의 벨트", "엘의 빛을 찾는 자가 착용한다. 그것은 가장 어두운 꿈 속에서 위로를 제공한다.");
        AddKO(191, "악몽 사냥꾼의 벨트", "패배한 악몽으로 만든 벨트. 그들의 정수가 착용자를 강화한다.");
        AddKO(192, "오컬트 벨트", "달의 오컬트 협회에서 온 벨트. 그것은 마법 에너지를 증폭시킨다.");
        AddKO(193, "성스러운 불꽃 벨트", "베스퍼의 기사단에 의해 축복받은 벨트. 그것은 보호의 불꽃으로 타오른다.");
        AddKO(194, "결투자의 검 벨트", "아스트리드의 귀족 전사가 착용하는 우아한 벨트. 그것은 검과 명예를 운반한다.");
        AddKO(195, "영혼 직조자의 밧줄", "숲의 영혼들이 나키아를 위해 짠 벨트. 그것은 자연의 에너지로 윙윙거린다.");
        AddKO(196, "암살자의 도구 벨트", "숨겨진 구획이 있는 벨트. 쉘의 조종자들은 그것들을 사용하여 자신의 도구를 장비한다.");
        AddKO(197, "건맨의 홀스터", "라세르타의 화기를 위해 설계된 벨트. 빠른 뽑기, 더 빠른 살인.");
        AddKO(198, "엘의 성스러운 벨트", "엘 자신에 의해 축복받은 벨트. 그 빛이 영혼을 모든 어둠에 대해 고정한다.");
        AddKO(199, "악몽 군주의 사슬", "악몽 군주의 사슬에서 단조된 벨트. 그것은 공포를 착용자의 의지에 묶는다.");
        AddKO(200, "원시 꿈의 벨트", "첫 번째 꿈에서 온 벨트. 그것은 창조 자체의 메아리를 포함한다.");
        AddKO(201, "세계수 뿌리 벨트", "세계수의 뿌리에서 자라는 벨트. 나키아의 숲이 그 창조를 축복했다.");
        AddKO(202, "아스트리드의 가보", "아스트리드 가족의 가문 벨트. 미스트의 피가 그 직물에 짜여져 있다.");
        AddKO(203, "황혼 종교재판관의 벨트", "베스퍼의 최고 수준의 벨트. 그것은 그 앞에 서는 모든 자를 심판한다.");
        
        // Armor Sets - Bone Sentinel (204-215)
        AddKO(204, "뼈 보초의 마스크", "패배한 악몽의 두개골에서 조각된 마스크. 그 빈 눈이 환상을 통해 본다.");
        AddKO(205, "영혼계 수호자의 헬름", "나키아의 숲의 영혼들로 주입된 헬름. 보호의 속삭임이 착용자를 둘러싼다.");
        AddKO(206, "악몽 군주의 왕관", "정복된 악몽에서 단조된 왕관. 착용자가 공포 자체를 지배한다.");
        AddKO(207, "뼈 보초의 조끼", "꿈 속에서 죽은 생물의 갈비뼈에서 온 갑옷. 그들의 보호는 죽음을 넘어 계속된다.");
        AddKO(208, "영혼계 수호자의 흉갑", "결정화된 꿈으로 짠 흉갑. 그것은 움직이고 오는 타격에 적응한다.");
        AddKO(209, "악몽 군주의 흉갑", "어둠의 에너지로 맥동하는 갑옷. 악몽이 그 착용자 앞에서 절한다.");
        AddKO(210, "뼈 보초의 다리갑옷", "악몽의 뼈로 강화된 다리 갑옷. 그것들은 결코 피로하지 않고 결코 양보하지 않는다.");
        AddKO(211, "영혼계 수호자의 다리 갑옷", "숲의 영혼들에 의해 축복받은 다리 갑옷. 착용자가 잎을 통해 부는 바람처럼 움직인다.");
        AddKO(212, "악몽 군주의 다리 갑옷", "그림자를 마시는 다리 갑옷. 어둠이 각 걸음에 힘을 준다.");
        AddKO(213, "뼈 보초의 부츠", "꿈의 영역에서 조용히 걷기 위한 부츠. 죽은 자는 소리를 내지 않는다.");
        AddKO(214, "영혼계 수호자의 철 부츠", "꿈의 영역에 흔적을 남기지 않는 부츠. 악몽 사냥꾼에게 완벽하다.");
        AddKO(215, "악몽 군주의 부츠", "세계를 가로지르는 부츠. 현실이 각 걸음 아래에서 구부러진다.");
        
        // Aurena Armor (216-243)
        AddKO(216, "아우레나의 견습생 모자", "달의 오컬트 초보자가 착용하는 단순한 모자.");
        AddKO(217, "아우레나의 신비로운 터번", "약한 달빛 에너지로 주입된 터번.");
        AddKO(218, "아우레나의 매혹적인 왕관", "착용자의 마법 친화력을 향상시키는 왕관.");
        AddKO(219, "아우레나의 고대 왕관", "세대를 통해 전해진 고대 유물.");
        AddKO(220, "아우레나의 매의 헤드밴드", "하늘의 영혼들에 의해 축복받은 헤드밴드.");
        AddKO(221, "아우레나의 힘의 왕관", "오컬트 힘을 발산하는 강력한 왕관.");
        AddKO(222, "아우레나의 달빛 왕관", "달의 오컬트 마스터의 전설적인 왕관.");
        AddKO(223, "아우레나의 파란 로브", "달의 마법사가 착용하는 단순한 로브.");
        AddKO(224, "아우레나의 마법사의 로브", "실천적인 마법사들이 사랑하는 로브.");
        AddKO(225, "아우레나의 주문을 짠 로브", "마법의 실로 짠 로브.");
        AddKO(226, "아우레나의 계몽의 로브", "달빛 지혜를 인도하는 로브.");
        AddKO(227, "아우레나의 꿈의 포장", "결정화된 꿈으로 짠 포장.");
        AddKO(228, "아우레나의 왕실 비늘 로브", "달의 왕실에 적합한 로브.");
        AddKO(229, "아우레나의 얼음 여왕의 로브", "얼음 여왕 자신의 전설적인 로브.");
        AddKO(230, "아우레나의 파란 다리갑옷", "야망 있는 마법사가 착용하는 단순한 다리갑옷.");
        AddKO(231, "아우레나의 섬유 스커트", "자유로운 움직임을 허용하는 가벼운 스커트.");
        AddKO(232, "아우레나의 엘프 다리갑옷", "엘프에 의해 설계된 우아한 다리갑옷.");
        AddKO(233, "아우레나의 계몽의 다리갑옷", "지혜에 의해 축복받은 다리갑옷.");
        AddKO(234, "아우레나의 빙하 스커트", "서리 마법으로 주입된 스커트.");
        AddKO(235, "아우레나의 서리 큐로트", "추위의 힘을 발산하는 큐로트.");
        AddKO(236, "아우레나의 화려한 다리갑옷", "비교할 수 없는 우아함의 전설적인 다리갑옷.");
        AddKO(237, "부드러운 달빛 슬리퍼", "달의 오컬트 수습생이 착용하는 부드러운 신발. 그것들은 각 걸음을 달빛으로 완충한다.");
        AddKO(238, "순례자의 샌들", "협회의 장로들에 의해 축복받은 단순한 샌들. 아우레나는 여전히 그들의 따뜻함을 기억한다.");
        AddKO(239, "동양의 신비 신발", "먼 땅에서 온 이국적인 신발. 그것들은 대지를 통해 치유 에너지를 인도한다.");
        AddKO(240, "꿈의 방랑자의 은총", "깨어있는 세계와 꿈의 세계 사이를 걷는 부츠. 길 잃은 영혼을 찾는 치유자에게 완벽하다.");
        AddKO(241, "영혼 방랑자의 부츠", "죽은 치유자들의 정수로 주입된 부츠. 그들의 지혜가 각 걸음을 인도한다.");
        AddKO(242, "영혼 추적자의 부츠", "부상자를 추적하는 어두운 부츠. 아우레나는 그것들을 사용하여 도움이 필요한 자를 찾는다... 또는 그녀를 해친 자.");
        AddKO(243, "타락한 현자의 날개", "순수한 마음에 비행 능력을 부여한다고 전해지는 전설적인 부츠. 아우레나의 추방이 그녀의 자격을 증명했다.");
        
        // Bismuth Armor (244-271)
        AddKO(244, "비스무트의 원뿔 모자", "수정 견습생을 위한 단순한 모자.");
        AddKO(245, "비스무트의 에메랄드 모자", "에메랄드 수정으로 장식된 모자.");
        AddKO(246, "비스무트의 빙하 마스크", "얼어붙은 수정으로 만든 마스크.");
        AddKO(247, "비스무트의 알라하림 마스크", "수정 힘의 고대 마스크.");
        AddKO(248, "비스무트의 프리즘 헬름", "빛을 무지개로 굴절시키는 헬름.");
        AddKO(249, "비스무트의 반사 왕관", "마법 에너지를 반사하는 왕관.");
        AddKO(250, "비스무트의 백열 왕관", "수정의 불꽃으로 타는 전설적인 왕관.");
        AddKO(251, "비스무트의 얼음 로브", "수정 마법에 의해 냉각된 로브.");
        AddKO(252, "비스무트의 수도사의 로브", "명상을 위한 단순한 로브.");
        AddKO(253, "비스무트의 빙하 로브", "영원한 서리에 얼어붙은 로브.");
        AddKO(254, "비스무트의 수정 갑옷", "살아있는 수정에서 형성된 갑옷.");
        AddKO(255, "비스무트의 프리즘 갑옷", "빛 자체를 구부리는 갑옷.");
        AddKO(256, "비스무트의 얼어붙은 판금 갑옷", "영원한 서리의 판금 갑옷.");
        AddKO(257, "비스무트의 성스러운 판금 갑옷", "수정 신들에 의해 축복받은 전설적인 갑옷.");
        AddKO(258, "비스무트의 사슬갑옷 다리갑옷", "수정 링크가 있는 기본 다리갑옷.");
        AddKO(259, "비스무트의 섬유 다리갑옷", "수정 마법사를 위한 가벼운 다리갑옷.");
        AddKO(260, "비스무트의 에메랄드 다리갑옷", "에메랄드 수정으로 장식된 다리갑옷.");
        AddKO(261, "비스무트의 기이한 바지", "기이한 에너지로 맥동하는 바지.");
        AddKO(262, "비스무트의 프리즘 다리갑옷", "프리즘 빛으로 빛나는 다리갑옷.");
        AddKO(263, "비스무트의 알라하림 다리갑옷", "알라하림의 고대 다리갑옷.");
        AddKO(264, "비스무트의 매 다리갑옷", "매 기사단의 전설적인 다리갑옷.");
        AddKO(265, "학자의 슬리퍼", "도서관에서 긴 시간 일하는 데 편안한 신발. 지식이 그 밑창을 통해 흐른다.");
        AddKO(266, "알라하림의 포장", "잃어버린 문명에서 온 고대 포장. 그것들은 잊혀진 주문을 속삭인다.");
        AddKO(267, "얼음 신발", "영원한 얼음에서 조각된 부츠. 비스무트가 걷는 곳에 서리 패턴을 남긴다.");
        AddKO(268, "서리 꽃의 걸음", "얼음 수정으로 피는 우아한 부츠. 각 걸음이 서리 정원을 만든다.");
        AddKO(269, "수정 공명의 부츠", "마법 에너지를 증폭시키는 부츠. 수정이 미개발된 힘으로 윙윙거린다.");
        AddKO(270, "프리즘 오컬트의 부츠", "빛을 순수한 마법 에너지로 굴절시키는 부츠. 현실이 각 걸음 주위에서 구부러진다.");
        AddKO(271, "공허의 방랑자의 부츠", "공허 자체를 가로지르는 전설적인 부츠. 비스무트는 다른 자가 상상할 수 없는 길을 볼 수 있다.");
        
        // Lacerta Armor (272-299)
        AddKO(272, "리자드맨의 가죽 헬름", "용 전사를 위한 기본 헬름.");
        AddKO(273, "리자드맨의 사슬갑옷 헬름", "견고한 사슬갑옷 헬름.");
        AddKO(274, "리자드맨의 전사의 헬름", "경험이 풍부한 전사가 착용하는 헬름.");
        AddKO(275, "리자드맨의 용 비늘 헬름", "용 비늘에서 단조된 헬름.");
        AddKO(276, "리자드맨의 왕실 헬름", "용의 왕실에 적합한 헬름.");
        AddKO(277, "리자드맨의 엘리트 용인 헬름", "용인 기사단의 엘리트 헬름.");
        AddKO(278, "리자드맨의 금 헬름", "용왕의 전설적인 금 헬름.");
        AddKO(279, "리자드맨의 가죽 갑옷", "용 전사를 위한 기본 갑옷.");
        AddKO(280, "리자드맨의 비늘 갑옷", "비늘로 강화된 갑옷.");
        AddKO(281, "리자드맨의 기사의 갑옷", "용 기사의 무거운 갑옷.");
        AddKO(282, "리자드맨의 용 비늘 사슬갑옷", "용 비늘에서 단조된 사슬갑옷.");
        AddKO(283, "리자드맨의 왕실 용인 사슬갑옷", "용인 기사단의 왕실 사슬갑옷.");
        AddKO(284, "리자드맨의 매 판금 갑옷", "매 기사단의 판금 갑옷.");
        AddKO(285, "리자드맨의 금 갑옷", "용왕의 전설적인 금 갑옷.");
        AddKO(286, "리자드맨의 가죽 다리갑옷", "용 전사를 위한 기본 다리갑옷.");
        AddKO(287, "리자드맨의 리벳 다리갑옷", "리벳으로 강화된 다리갑옷.");
        AddKO(288, "리자드맨의 기사의 다리갑옷", "용 기사의 무거운 다리갑옷.");
        AddKO(289, "리자드맨의 용 비늘 다리갑옷", "용 비늘에서 단조된 다리갑옷.");
        AddKO(290, "리자드맨의 왕관 다리갑옷", "왕실에 적합한 다리갑옷.");
        AddKO(291, "리자드맨의 화려한 다리갑옷", "마스터 크래프트의 화려한 다리갑옷.");
        AddKO(292, "리자드맨의 금 다리갑옷", "용왕의 전설적인 금 다리갑옷.");
        AddKO(293, "닳은 가죽 부츠", "많은 전투를 목격한 단순한 부츠. 그것들은 모험의 향기를 운반한다.");
        AddKO(294, "패치 전투 부츠", "무수한 번 수리된 부츠. 각 패치가 생존의 이야기를 말한다.");
        AddKO(295, "강철 전사의 부츠", "전투를 위해 단조된 무거운 부츠. 그것들은 전투 중에 라세르타를 지면에 고정한다.");
        AddKO(296, "수호자의 다리갑옷", "엘리트 보호자가 착용하는 부츠. 그것들은 결코 후퇴하지 않고 결코 항복하지 않는다.");
        AddKO(297, "용 비늘 다리갑옷", "용 비늘로 만든 부츠. 그것들은 착용자에게 용의 강인함을 부여한다.");
        AddKO(298, "용인 군벌의 부츠", "고대 용인 장군의 부츠. 그것들은 어떤 전장에서도 두려운 존재다.");
        AddKO(299, "금 챔피언의 부츠", "시대의 최고 전사가 착용하는 전설적인 부츠. 라세르타의 운명이 기다린다.");
        
        // Mist Armor (300-327)
        AddKO(300, "미스트의 터번", "정체성을 숨기는 단순한 터번.");
        AddKO(301, "미스트의 경량 터번", "경량이고 통기성 있는 터번.");
        AddKO(302, "미스트의 야간 시력 터번", "야간 시력을 향상시키는 터번.");
        AddKO(303, "미스트의 번개 헤드밴드", "전기 에너지로 가득 찬 헤드밴드.");
        AddKO(304, "미스트의 코브라 후드", "뱀처럼 치명적인 후드.");
        AddKO(305, "미스트의 지옥 추적자의 마스크", "사냥감을 추적하는 마스크.");
        AddKO(306, "미스트의 어둠의 속삭임", "죽음을 속삭이는 전설적인 마스크.");
        AddKO(307, "미스트의 가죽 하네스", "민첩성을 위한 경량 하네스.");
        AddKO(308, "미스트의 레인저 망토", "빠른 움직임을 위한 망토.");
        AddKO(309, "미스트의 한밤중 튜닉", "야간 타격을 위한 튜닉.");
        AddKO(310, "미스트의 번개 로브", "번개로 가득 찬 로브.");
        AddKO(311, "미스트의 전압 갑옷", "전기 에너지로 맥동하는 갑옷.");
        AddKO(312, "미스트의 마스터 궁수의 갑옷", "궁수의 마스터의 갑옷.");
        AddKO(313, "미스트의 어둠 군주의 망토", "어둠 군주의 전설적인 망토.");
        AddKO(314, "미스트의 레인저 다리갑옷", "레인저가 착용하는 다리갑옷.");
        AddKO(315, "미스트의 정글 생존자의 다리갑옷", "정글 생존자의 다리갑옷.");
        AddKO(316, "미스트의 한밤중 사롱", "한밤중 작전을 위한 사롱.");
        AddKO(317, "미스트의 번개 다리갑옷", "번개로 가득 찬 다리갑옷.");
        AddKO(318, "미스트의 메뚜기 다리갑옷", "놀라운 도약을 위한 다리갑옷.");
        AddKO(319, "미스트의 녹색 악마 다리갑옷", "악마의 속도의 다리갑옷.");
        AddKO(320, "미스트의 영혼 방랑자의 다리갑옷", "세계 사이를 걷는 전설적인 다리갑옷.");
        AddKO(321, "임시 정찰 부츠", "파편에서 조립된 부츠. 가볍고 조용하며 정찰에 완벽하다.");
        AddKO(322, "늪지 달리기 부츠", "어려운 지형을 가로지르는 방수 부츠. 미스트는 모든 지름길을 안다.");
        AddKO(323, "신속한 방랑자", "착용자의 걸음을 가속시키는 마법 부츠. 히트 앤 런 전술에 완벽하다.");
        AddKO(324, "수사슴 사냥꾼의 부츠", "사냥감을 추적하는 부츠.");
        AddKO(325, "번개 타격자의 부츠", "번개 에너지로 가득 찬 부츠.");
        AddKO(326, "불의 방랑자 전투 부츠", "불을 통해 걸을 수 있는 부츠.");
        AddKO(327, "고난의 짓밟기", "모든 고난을 짓밟는 전설적인 부츠.");
        
        // Nachia Armor (328-355)
        AddKO(328, "나키아의 모피 모자", "숲 사냥꾼을 위한 단순한 모자.");
        AddKO(329, "나키아의 깃털 머리장식", "깃털로 장식된 머리장식.");
        AddKO(330, "나키아의 샤먼 마스크", "영적 힘의 마스크.");
        AddKO(331, "나키아의 대지의 후드", "대지의 영혼들에 의해 축복받은 후드.");
        AddKO(332, "나키아의 자연의 헬름", "자연의 힘으로 주입된 헬름.");
        AddKO(333, "나키아의 나무 왕관", "고목에서 자란 왕관.");
        AddKO(334, "나키아의 잎 왕관", "숲의 수호자의 전설적인 왕관.");
        AddKO(335, "나키아의 모피 갑옷", "숲의 생물로 만든 갑옷.");
        AddKO(336, "나키아의 원주민 갑옷", "숲 부족의 전통 갑옷.");
        AddKO(337, "나키아의 녹색 나무 코트", "살아있는 덩굴로 짠 코트.");
        AddKO(338, "나키아의 대지의 망토", "대지의 영혼들에 의해 축복받은 망토.");
        AddKO(339, "나키아의 자연의 포옹", "자연과 하나가 된 갑옷.");
        AddKO(340, "나키아의 늪지 둥지 갑옷", "깊은 늪지에서 온 갑옷.");
        AddKO(341, "나키아의 잎 로브", "숲의 수호자의 전설적인 로브.");
        AddKO(342, "나키아의 매머드 모피 반바지", "매머드 모피로 만든 따뜻한 반바지.");
        AddKO(343, "나키아의 가죽 반바지", "전통적인 사냥 다리갑옷.");
        AddKO(344, "나키아의 수사슴 다리갑옷", "수사슴 가죽으로 만든 다리갑옷.");
        AddKO(345, "나키아의 대지의 다리갑옷", "대지의 영혼들에 의해 축복받은 다리갑옷.");
        AddKO(346, "나키아의 잎 다리갑옷", "마법의 잎으로 짠 다리갑옷.");
        AddKO(347, "나키아의 멧돼지인 허리천", "강력한 멧돼지인에서 온 허리천.");
        AddKO(348, "나키아의 피의 사냥 다리갑옷", "피의 사냥의 전설적인 다리갑옷.");
        AddKO(349, "모피 안감 부츠", "따뜻하고 편안한 부츠.");
        AddKO(350, "오소리 가죽 부츠", "오소리 가죽으로 만든 부츠.");
        AddKO(351, "코브라 타격 부츠", "뱀처럼 빠른 부츠.");
        AddKO(352, "숲 추적자의 포장", "숲에서 조용히 움직이는 포장.");
        AddKO(353, "대지의 사냥꾼의 부츠", "사냥감을 추적하는 부츠.");
        AddKO(354, "열병 꽃 레인저의 부츠", "열병 꽃으로 장식된 부츠. 그것들은 열광적인 속도와 치명적인 정확도를 부여한다.");
        AddKO(355, "피의 포식자의 부츠", "무수한 사냥의 피로 물든 전설적인 부츠. 나키아가 최고의 포식자가 되었다.");
        
        // Shell Armor (356-383)
        AddKO(356, "쉘의 손상된 헬름", "지하 세계에서 온 낡은 헬름.");
        AddKO(357, "쉘의 부서진 마스크", "여전히 보호를 제공하는 부서진 마스크.");
        AddKO(358, "쉘의 뼈 군주의 헬름", "뼈 군주의 유물에서 단조된 헬름.");
        AddKO(359, "쉘의 해골 헬름", "뼈에서 형성된 헬름.");
        AddKO(360, "쉘의 죽음의 헬름", "죽음 자체의 헬름.");
        AddKO(361, "쉘의 녹크페라투 해골 수호자", "뱀파이어 뼈의 헬름.");
        AddKO(362, "쉘의 악마의 헬름", "악마 군주의 전설적인 헬름.");
        AddKO(363, "쉘의 장례 포장", "무덤에서 온 포장.");
        AddKO(364, "쉘의 오래된 망토", "고대에서 온 낡은 망토.");
        AddKO(365, "쉘의 녹크페라투 뼈 망토", "뼈로 짠 망토.");
        AddKO(366, "쉘의 영혼 망토", "불안한 영혼의 망토.");
        AddKO(367, "쉘의 죽음의 로브", "죽음의 로브.");
        AddKO(368, "쉘의 지하 세계의 로브", "심연에서 온 로브.");
        AddKO(369, "쉘의 악마의 갑옷", "악마 군주의 전설적인 갑옷.");
        AddKO(370, "쉘의 손상된 앞치마", "손상되었지만 유용한 앞치마.");
        AddKO(371, "쉘의 변이된 뼈 스커트", "변이된 뼈로 만든 스커트.");
        AddKO(372, "쉘의 녹크페라투 가시 포장", "가시가 있는 포장.");
        AddKO(373, "쉘의 녹크페라투 살점 수호자", "보존된 살점으로 만든 보호 장비.");
        AddKO(374, "쉘의 피의 다리갑옷", "피로 물든 다리갑옷.");
        AddKO(375, "쉘의 녹크페라투 피의 방랑자", "피 속을 걷는 다리갑옷.");
        AddKO(376, "쉘의 악마의 다리갑옷", "악마 군주의 전설적인 다리갑옷.");
        AddKO(377, "장갑 다리갑옷", "발 아래의 모든 것을 분쇄하는 무거운 금속 부츠. 방어가 최고의 공격이다.");
        AddKO(378, "항해자의 부츠", "모든 폭풍을 견디는 견고한 부츠. 그것들은 쉘을 전투의 흐름에 고정한다.");
        AddKO(379, "심연의 다리갑옷", "바다 깊은 곳에서 단조된 부츠. 그것들은 모든 압력을 견딘다.");
        AddKO(380, "뼈 분쇄 부츠", "괴물의 뼈로 강화된 부츠. 각 걸음이 위협이다.");
        AddKO(381, "마그마 요새의 부츠", "화산의 불꽃에서 단조된 부츠. 그것들은 공격자를 태우는 열을 발산한다.");
        AddKO(382, "피의 짓밟기 부츠", "파괴의 흔적을 남기는 잔인한 부츠. 적은 그것들과 싸우는 것을 두려워한다.");
        AddKO(383, "불굴의 수호자의 부츠", "파괴 불가능한 방어자의 전설적인 부츠. 쉘이 움직이지 않는 요새가 된다.");
        
        // Vesper Armor (384-411)
        AddKO(384, "베스퍼의 마녀 모자", "꿈의 직조자를 위한 단순한 모자.");
        AddKO(385, "베스퍼의 기괴한 후드", "비밀을 속삭이는 후드.");
        AddKO(386, "베스퍼의 기이한 후드", "기이한 에너지로 맥동하는 후드.");
        AddKO(387, "베스퍼의 악몽 에너지 후드", "악몽 에너지의 후드.");
        AddKO(388, "베스퍼의 절망의 포장", "절망을 먹는 포장.");
        AddKO(389, "베스퍼의 어둠 마법사의 왕관", "어둠 마법의 왕관.");
        AddKO(390, "베스퍼의 페룬브라스 모자", "어둠의 마스터의 전설적인 모자.");
        AddKO(391, "베스퍼의 빨간 로브", "야망 있는 꿈의 마법사를 위한 로브.");
        AddKO(392, "베스퍼의 영혼의 속박", "영혼을 인도하는 속박.");
        AddKO(393, "베스퍼의 에너지 로브", "에너지로 딱딱 소리를 내는 로브.");
        AddKO(394, "베스퍼의 유령 스커트", "유령 에너지로 짠 스커트.");
        AddKO(395, "베스퍼의 영혼 망토", "영혼을 포함하는 망토.");
        AddKO(396, "베스퍼의 영혼 포장", "포획된 영혼의 포장.");
        AddKO(397, "베스퍼의 오컬트 드래곤 로브", "오컬트 드래곤의 전설적인 로브.");
        AddKO(398, "베스퍼의 영혼 다리갑옷", "영혼 에너지로 주입된 다리갑옷.");
        AddKO(399, "베스퍼의 이국적인 다리갑옷", "먼 영역에서 온 이국적인 다리갑옷.");
        AddKO(400, "베스퍼의 영혼 다리 갑옷", "영혼의 힘을 인도하는 다리갑옷.");
        AddKO(401, "베스퍼의 피의 바지", "피 마법으로 염색된 바지.");
        AddKO(402, "베스퍼의 마그마 다리갑옷", "마법의 불꽃에서 단조된 다리갑옷.");
        AddKO(403, "베스퍼의 지혜 다리갑옷", "고대 지혜의 다리갑옷.");
        AddKO(404, "베스퍼의 고대인의 바지", "고대인에서 온 전설적인 바지.");
        AddKO(405, "수습생의 슬리퍼", "신전의 신입 회원이 착용하는 부드러운 슬리퍼. 그것들은 각 걸음에 기도를 운반한다.");
        AddKO(406, "신전의 신발", "엘의 종의 전통 신발. 그것들은 베스퍼를 그녀의 신앙에 고정한다.");
        AddKO(407, "기이한 수도사의 부츠", "금지된 텍스트를 연구하는 수도사가 착용하는 부츠. 지식과 신앙이 얽힌다.");
        AddKO(408, "노움의 축복 포장", "노움 장인에 의해 마법을 부여받은 포장. 기술과 신성이 만난다.");
        AddKO(409, "뱀파이어 비단 슬리퍼", "뱀파이어 비단으로 짠 우아한 슬리퍼. 그것들은 대지 자체에서 생명을 끌어낸다.");
        AddKO(410, "악마 살해자의 슬리퍼", "악을 파괴하기 위해 축복받은 녹색 슬리퍼. 그것들은 각 걸음에서 악마를 태운다.");
        AddKO(411, "악몽 추방자의 부츠", "악몽을 가로지르며 무고한 자를 구하는 전설적인 부츠. 베스퍼의 궁극의 사명.");
        
        // Yubar Armor (412-439)
        AddKO(412, "유바르의 부족 마스크", "부족 전사를 위한 마스크.");
        AddKO(413, "유바르의 바이킹 헬름", "북쪽 전사를 위한 헬름.");
        AddKO(414, "유바르의 뿔 헬름", "무서운 뿔이 있는 헬름.");
        AddKO(415, "유바르의 불굴의 익스 머리장식", "익스 부족을 위한 머리장식.");
        AddKO(416, "유바르의 불의 공포 머리장식", "공포로 타는 머리장식.");
        AddKO(417, "유바르의 불굴의 익스 헬름", "익스 전사를 위한 헬름.");
        AddKO(418, "유바르의 종말의 얼굴", "종말의 전설적인 얼굴.");
        AddKO(419, "유바르의 곰 가죽", "강력한 곰에서 온 갑옷.");
        AddKO(420, "유바르의 매머드 모피 망토", "매머드에서 온 망토.");
        AddKO(421, "유바르의 불굴의 익스 로브", "익스 부족을 위한 로브.");
        AddKO(422, "유바르의 마그마 코트", "마그마에서 단조된 코트.");
        AddKO(423, "유바르의 불굴의 익스 흉갑", "익스 전사를 위한 흉갑.");
        AddKO(424, "유바르의 용암 판금 갑옷", "용암의 판금 갑옷.");
        AddKO(425, "유바르의 불의 거인의 갑옷", "불의 거인의 전설적인 갑옷.");
        AddKO(426, "유바르의 불굴의 익스 허리 갑옷", "익스 부족을 위한 다리 갑옷.");
        AddKO(427, "유바르의 변이된 가죽 바지", "변이된 가죽으로 만든 바지.");
        AddKO(428, "유바르의 불굴의 익스 다리 스커트", "익스 전사를 위한 다리 스커트.");
        AddKO(429, "유바르의 드워프 다리갑옷", "견고한 드워프 다리갑옷.");
        AddKO(430, "유바르의 합금 다리갑옷", "합금으로 강화된 다리갑옷.");
        AddKO(431, "유바르의 판금 다리갑옷", "무거운 판금 다리갑옷.");
        AddKO(432, "유바르의 노움 다리갑옷", "노움 공예의 전설적인 다리갑옷.");
        AddKO(433, "임시 전투 부츠", "전장의 파편에서 조립된 부츠. 유바르는 찾은 것으로 만든다.");
        AddKO(434, "부족 포장", "유바르의 사람들의 전통 포장. 그것들은 그를 조상에게 연결한다.");
        AddKO(435, "수사슴 전사의 다리갑옷", "수사슴 뿔로 장식된 부츠. 그것들은 숲의 왕의 힘을 부여한다.");
        AddKO(436, "송곳니 짓밟기의 부츠", "송곳니 모양의 가시가 있는 잔인한 부츠. 그것들은 적을 발 아래에 짓밟는다.");
        AddKO(437, "피의 버서커 부츠", "피처럼 빨간 부츠, 유바르의 분노를 타오르게 한다. 고통이 힘이 된다.");
        AddKO(438, "영혼의 짓밟기", "쓰러진 적의 영혼을 수확하는 부츠. 그것들의 힘이 각 전투에서 성장한다.");
        AddKO(439, "귀향 챔피언의 부츠", "정신적으로 유바르를 고향으로 데려가는 전설적인 부츠. 그의 조상들이 그와 함께 싸운다.");
        
        // Legendary Items (440-443)
        AddKO(440, "꿈의 방랑자의 날개 헬름", "꿈의 방랑자에게 비행 능력을 부여하는 전설적인 헬름. 가장 가까운 적을 자동으로 공격한다.");
        AddKO(441, "꿈의 마법 판금 갑옷", "모든 영역의 꿈의 방랑자를 보호하는 전설적인 갑옷. 가장 가까운 적을 자동으로 조준한다.");
        AddKO(442, "심연의 다리갑옷", "꿈의 가장 깊은 부분에서 단조된 전설적인 다리갑옷.");
        AddKO(443, "수상 보행 부츠", "착용자가 물 위를 걸을 수 있게 하는 전설적인 부츠. 모든 영역의 꿈의 방랑자가 찾는 보물.");
        
        // Consumables (444-447)
        AddKO(444, "몽상가의 토닉", "깨어있는 세계에서 발견한 약초로 만든 간단한 치료제. 가장 희미한 꿈도 한 방울의 희망에서 시작된다.");
        AddKO(445, "몽상의 정수", "평화로운 잠의 기억에서 증류됨. 마시는 자는 잊혀진 꿈의 온기가 상처를 씻어내는 것을 느낀다.");
        AddKO(446, "명석한 활력", "스무디가 별의 먼지와 악몽 조각을 사용해 만듦. 액체는 천 개의 잠든 별빛으로 빛난다.");
        AddKO(447, "엘의 영약", "엘 자신의 빛으로 축복받은 신성한 음료. 베스퍼 기사단은 그 레시피를 질투하듯 지킨다. 가장 어두운 악몽이 남긴 상처도 치유할 수 있기 때문이다.");
        
        // Universal Legendary Belt (448)
        AddKO(448, "무한한 꿈의 벨트", "착용자를 영원한 꿈의 순환에 묶는 전설적인 벨트. 악몽을 정복할 때마다 힘이 성장한다.");
        
        // Hero-Specific Legendary Belts (449-457)
        AddKO(449, "오우레나의 생명 직조자의 벨트", "오우레나의 도난당한 생명력 연구로부터 단조된 벨트. 생명과 죽음을 완벽한 균형으로 엮는다.");
        AddKO(450, "비스무스의 수정 허리띠", "맹인 소녀의 시각과 함께 맥동하는 순수한 수정 벨트. 눈이 볼 수 없는 것을 본다.");
        AddKO(451, "라세르타의 진홍색 눈의 벨트", "라세르타의 전설적인 시선을 이끄는 벨트. 그 경계의 힘에서 벗어나는 표적은 없다.");
        AddKO(452, "미스트의 결투자 챔피언 벨트", "아스트리드 가문의 궁극의 벨트. 미스트의 명예가 모든 실에 짜여 있다.");
        AddKO(453, "나치아의 세계수의 벨트", "세계수 그 자체에서 자란 벨트. 자연의 분노가 그 안을 흐른다.");
        AddKO(454, "하스크의 부서지지 않는 사슬", "부서지지 않는 결심으로부터 단조된 벨트. 하스크의 결심이 그 힘이다.");
        AddKO(455, "셸의 그림자 암살자의 벨트", "완벽한 어둠의 벨트. 셸의 조종자들은 그 의지를 깰 수 없었다.");
        AddKO(456, "베스퍼의 황혼의 종교재판관 벨트", "엘의 기사단의 최고 등급 벨트. 악몽을 심판하고 어둠을 추방한다.");
        AddKO(457, "유바르의 조상 챔피언 벨트", "유바르의 조상을 부르는 벨트. 그들의 힘이 모든 섬유를 통해 흐른다.");
        
        // Universal Legendary Ring (458)
        AddKO(458, "영원한 꿈의 반지", "착용자를 무한한 꿈의 영역에 연결하는 전설적인 반지. 그 힘은 모든 경계를 초월한다.");
        
        // Hero-Specific Legendary Rings (459-467)
        AddKO(459, "오우레나의 대현자의 반지", "달의 아르카눔의 대현자의 반지. 오우레나는 그들의 위선의 증거로 그것을 가져갔다.");
        AddKO(460, "비스무스의 보석 심장의 반지", "순수한 수정 마법의 반지. 맹인 소녀의 변형된 본질과 함께 맥동한다.");
        AddKO(461, "라세르타의 왕실 처형인의 반지", "왕실 경비대의 엘리트 처형인의 반지. 라세르타는 무수한 전투를 통해 그것을 획득했다.");
        AddKO(462, "미스트의 아스트리드 유산의 반지", "아스트리드 가문의 조상의 반지. 미스트의 혈통이 그 금속을 통해 흐른다.");
        AddKO(463, "나치아의 숲의 수호자의 반지", "나치아의 숲의 정령들에게 축복받은 반지. 자연의 힘이 그 본질이다.");
        AddKO(464, "하스크의 굴복하지 않는 결의의 반지", "부서지지 않는 의지로부터 단조된 반지. 하스크의 결심이 그 힘이다.");
        AddKO(465, "셸의 완벽한 그림자의 반지", "절대적인 어둠의 반지. 셸의 조종자들은 그 힘을 제어할 수 없었다.");
        AddKO(466, "베스퍼의 성스러운 불꽃의 반지", "엘의 성스러운 불꽃의 순수한 본질을 포함하는 반지. 모든 어둠을 추방한다.");
        AddKO(467, "유바르의 부족 챔피언 반지", "유바르의 조상의 힘을 이끄는 반지. 그들의 분노가 모든 타격을 강화한다.");
        
        // Universal Legendary Amulet (468)
        AddKO(468, "무한한 꿈의 부적", "착용자를 영원한 꿈의 순환에 묶는 전설적인 부적. 그 힘은 모든 경계를 초월한다.");
        
        // Hero-Specific Legendary Amulets (469-477)
        AddKO(469, "오우레나의 생명 직조자의 부적", "오우레나의 도난당한 생명력 연구로부터 단조된 부적. 생명과 죽음을 완벽한 균형으로 엮는다.");
        AddKO(470, "비스무스의 수정 심장", "맹인 소녀의 시각과 함께 맥동하는 순수한 수정 부적. 눈이 볼 수 없는 것을 본다.");
        AddKO(471, "라세르타의 진홍색 눈의 부적", "라세르타의 전설적인 시선을 이끄는 부적. 그 경계의 힘에서 벗어나는 표적은 없다.");
        AddKO(472, "미스트의 결투자 챔피언 부적", "아스트리드 가문의 궁극의 부적. 미스트의 명예가 모든 보석에 짜여 있다.");
        AddKO(473, "나치아의 세계수의 부적", "세계수 그 자체에서 자란 부적. 자연의 분노가 그 안을 흐른다.");
        AddKO(474, "하스크의 부서지지 않는 부적", "부서지지 않는 결심으로부터 단조된 부적. 하스크의 결심이 그 힘이다.");
        AddKO(475, "셸의 그림자 암살자의 부적", "완벽한 어둠의 부적. 셸의 조종자들은 그 의지를 깰 수 없었다.");
        AddKO(476, "베스퍼의 황혼의 종교재판관 부적", "엘의 기사단의 최고 등급 부적. 악몽을 심판하고 어둠을 추방한다.");
        AddKO(477, "유바르의 조상 챔피언 부적", "유바르의 조상을 부르는 부적. 그들의 힘이 모든 보석을 통해 흐른다.");
    }
    
    private static void AddKO(int id, string name, string desc)
    {
        _nameKO[id] = name;
        _descKO[id] = desc;
    }
    
    // ============================================================
    // ITALIAN TRANSLATIONS
    // ============================================================
    private static void InitializeIT()
    {
        _nameIT = new Dictionary<int, string>();
        _descIT = new Dictionary<int, string>();
        
        // Aurena Weapons (1-18)
        AddIT(1, "Artigli dell'Iniziato", "Artigli semplici dati ai nuovi membri della Società Arcanum Lunare. Aurena conservò i suoi anche dopo l'espulsione.");
        AddIT(2, "Artigli del Sapiente", "Artigli affilati su testi proibiti. I graffi che lasciano sussurrano segreti.");
        AddIT(3, "Artigli Lunari", "Artigli che brillano debolmente alla luce della luna. Canalizzano l'energia lunare in colpi devastanti.");
        AddIT(4, "Rasoi Arcanum", "Artigli incisi con rune curative - ora usati per lacerare invece di guarire.");
        AddIT(5, "Artigli dell'Esiliato", "Forgiati in segreto dopo il suo bando. Ogni taglio ricorda ciò che la Società le ha tolto.");
        AddIT(6, "Presagio dell'Eretico", "Artigli che appartenevano un tempo a un altro saggio espulso. Il loro spirito guida i colpi di Aurena.");
        AddIT(7, "Artigli Sacrificali", "Artigli che diventano più affilati man mano che la forza vitale di Aurena declina. La Società considerò questa ricerca troppo pericolosa.");
        AddIT(8, "Artigli Legati dal Gelo", "Artigli congelati con il ghiaccio del caveau più profondo della Società. Il loro tocco intorpidisce carne e spirito.");
        AddIT(9, "Laceratori Lunari", "Potenziati con incantesimi proibiti. Ogni ferita che infliggono guarisce gli alleati di Aurena.");
        AddIT(10, "Artigli Raggianti", "Artigli benedetti dalla luce solare rubata. La ricerca di Aurena sulla magia della luce ha infuriato sia i culti del sole che della luna.");
        AddIT(11, "Artigli di Brace", "Artigli accesi con la forza vitale estratta da sognatori consenzienti. Le fiamme non muoiono mai.");
        AddIT(12, "Artigli dell'Ombra", "Artigli immersi nell'essenza degli incubi. Aurena imparò che l'oscurità guarisce ciò che la luce non può.");
        AddIT(13, "Mietitore Glaciale", "Artigli scolpiti nel ghiaccio eterno. Congelano il sangue dei nemici e preservano la forza vitale di Aurena.");
        AddIT(14, "Artigli della Fenice", "Artigli forgiati nelle fiamme della fenice e temprati con il sangue stesso di Aurena. Bruciano con rabbia inestinguibile.");
        AddIT(15, "Laceratore del Vuoto", "Artigli capaci di lacerare la realtà stessa. La Società teme ciò che Aurena scoprirà dopo.");
        AddIT(16, "Artigli Celesti", "Artigli impregnati di pura luce stellare. Ogni colpo è una preghiera per la conoscenza che gli dei vogliono nascondere.");
        AddIT(17, "Artigli del Grande Saggio", "Artigli cerimoniali del grande saggio della Società. Aurena li usa come prova della loro ipocrisia.");
        AddIT(18, "Artigli del Tessitore della Vita", "Capolavoro di Aurena - artigli capaci di rubare la forza vitale senza distruggerla. La Società lo chiama eresia.");
        
        // Bismuth Weapons (19-36)
        AddIT(19, "Grimorio Vuoto", "Un libro di incantesimi vuoto in attesa di essere riempito. Come Bismuth stessa, le sue pagine contengono potenziale infinito.");
        AddIT(20, "Libro dell'Apprendista", "Un libro di incantesimi per principianti. La ragazza cieca che divenne Bismuth tracciava le sue pagine con le dita, sognando la magia.");
        AddIT(21, "Codice Cristallino", "Un libro le cui pagine sono fatte di gemme sottili. Risona con la natura cristallina di Bismuth.");
        AddIT(22, "Diario del Vagabondo", "Un diario di viaggio che registra le osservazioni della ragazza cieca che non vide mai ma seppe sempre.");
        AddIT(23, "Introduzione alle Gemme", "Una guida base sulla magia cristallina. Le sue parole brillano come il corpo arcobaleno di Bismuth.");
        AddIT(24, "Classico del Cieco", "Un libro scritto in lettere in rilievo per i ciechi. Bismuth lo legge per tatto e memoria.");
        AddIT(25, "Libro delle Parole di Brace", "Un libro le cui pagine bruciano eternamente. Le fiamme parlano con Bismuth in colori che sente, non vede.");
        AddIT(26, "Dizionario di Ghiaccio", "Un libro avvolto nel ghiaccio eterno. Le sue pagine ghiacciate preservano la conoscenza di prima dell'inizio dei sogni.");
        AddIT(27, "Manoscritto Raggiante", "Un libro che emana luce interiore. Prima che diventasse Bismuth, guidava la ragazza cieca attraverso l'oscurità.");
        AddIT(28, "Grimorio dell'Ombra", "Un libro che assorbe la luce. Le sue pagine oscure contengono segreti che persino Bismuth teme di leggere.");
        AddIT(29, "Libro di Incantesimi Vivente", "Un libro consapevole che scelse di fondersi con la ragazza cieca. Insieme divennero qualcosa di più grande.");
        AddIT(30, "Codice Prismatico", "Un libro che scompone la magia nei suoi colori componenti. Bismuth vede il mondo attraverso la luce che rifrange.");
        AddIT(31, "Cronaca del Vuoto", "Un libro che registra i ricordi di coloro che si persero nell'oscurità. Bismuth legge le loro storie per onorarli.");
        AddIT(32, "Grimorio del Cuore di Gemma", "Il libro di incantesimi originale che si fuse con la ragazza cieca. Le sue pagine pulsano con vita cristallina.");
        AddIT(33, "Libro della Luce Accecante", "Un libro così accecante che può accecare i vedenti. Per Bismuth, è solo calore.");
        AddIT(34, "Annali Legati dal Gelo", "Un libro antico congelato nel tempo. Le sue profezie raccontano di una gemma che cammina come una ragazza.");
        AddIT(35, "Libro Senza Nome", "Un libro senza nome, come la ragazza senza vista. Insieme trovarono la loro identità.");
        AddIT(36, "Classico dell'Inferno", "Un libro che brucia con la passione di un sognatore che rifiuta di lasciare che la cecità lo definisca.");
        
        // Lacerta Weapons (37-54)
        AddIT(37, "Fucile del Recluta", "Arma standard rilasciata ai recluti della Guardia Reale. Lacerta lo padroneggiò prima della fine della prima settimana di addestramento.");
        AddIT(38, "Fucile Lungo da Cacciatore", "Un fucile da caccia affidabile. Lacerta ne usò uno simile per mantenere la famiglia prima di unirsi alla Guardia.");
        AddIT(39, "Carabina da Pattuglia", "Compatta e affidabile. Perfetta per lunghe pattuglie ai confini del regno.");
        AddIT(40, "Fucile del Tiratore Scelto", "Un fucile bilanciato progettato per coloro che valorizzano la precisione sul potere.");
        AddIT(41, "Moschetto a Polvere Nera", "Vecchio ma affidabile. L'odore della polvere nera ricorda a Lacerta la sua prima caccia.");
        AddIT(42, "Fucile da Ricognizione", "Un fucile leggero per missioni di ricognizione. Velocità e furtività superano il potere grezzo.");
        AddIT(43, "Fucile della Guardia Reale", "L'arma che Lacerta portò durante il servizio. Ogni graffio racconta una storia.");
        AddIT(44, "Orgoglio del Cecchino", "Un fucile preciso per coloro che non mancano mai. L'occhio scarlatto di Lacerta vede ciò che altri non possono.");
        AddIT(45, "Fucile Lungo Fiammeggiante", "Caricato con proiettili incendiari. I nemici bruciano a lungo dopo l'impatto.");
        AddIT(46, "Carabina Morso del Gelo", "Spara proiettili raffreddati allo zero assoluto. I bersagli rallentano a passo di lumaca prima del colpo fatale.");
        AddIT(47, "Fucile della Notte", "Un'arma per cacciare nell'oscurità. I suoi colpi sono silenziosi come l'ombra.");
        AddIT(48, "Moschetto dell'Alba", "Benedetto dalla luce dell'alba. I suoi colpi penetrano l'oscurità e l'inganno.");
        AddIT(49, "Occhio Scarlatto", "Chiamato per lo sguardo leggendario di Lacerta. Nessun bersaglio sfugge alla vista di questo fucile.");
        AddIT(50, "Esecutore Reale", "Riservato all'élite della Guardia. Lacerta ricevette questo fucile dopo aver salvato il regno.");
        AddIT(51, "Cannone Solare", "Fucili che sparano luce concentrata. Ogni colpo è un'alba in miniatura.");
        AddIT(52, "Cacciatore del Vuoto", "Fucili che sparano proiettili di pura oscurità. I bersagli scompaiono senza traccia.");
        AddIT(53, "Zero Assoluto", "Il punto più freddo dell'esistenza. Anche il tempo si congela davanti ad esso.");
        AddIT(54, "Soffio del Drago", "Fucili caricati con proiettili esplosivi incendiari. I nemici del regno hanno imparato a temere il suo ruggito.");
        
        // Mist Weapons (55-70)
        AddIT(55, "Fioretto da Addestramento", "Spada da addestramento della giovinezza di Mist. Anche allora superò ogni istruttore ad Astrid.");
        AddIT(56, "Stocco Nobile", "Stocco che corrisponde al lignaggio nobile di Mist. Leggero, elegante, mortale.");
        AddIT(57, "Stocco del Duellante", "Stocco progettato per il combattimento uno contro uno. Mist non perse mai un duello.");
        AddIT(58, "Stocco Rapido", "Leggero come un'estensione del braccio di Mist.");
        AddIT(59, "Stocco di Astrid", "Forgiato nelle migliori fucine di Astrid. Simbolo dell'artigianato reale.");
        AddIT(60, "Stocco del Danzatore", "Stocco per coloro che trattano il combattimento come arte. I movimenti di Mist sono poetici.");
        AddIT(61, "Stocco Fiammeggiante", "Stocco circondato da fiamme. La rabbia di Mist è calda quanto la sua determinazione.");
        AddIT(62, "Stocco dell'Ombra", "Stocco che colpisce dall'oscurità. I nemici cadono prima di vedere Mist muoversi.");
        AddIT(63, "Stocco Morso del Gelo", "Stocco di acciaio ghiacciato. Il suo tocco intorpidisce corpo e spirito.");
        AddIT(64, "Grazia di Camilla", "Un dono da qualcuno che Mist apprezzava. Combatte per onorare la loro memoria.");
        AddIT(65, "Stocco di Parata", "Stocco progettato per difesa e attacco. Mist trasforma ogni attacco in opportunità.");
        AddIT(66, "Stocco Raggiante", "Stocco che emana luce interiore. Penetra bugie e ombre.");
        AddIT(67, "Stocco dell'Inferno", "Stocco che brucia con zelo inestinguibile. La determinazione di Mist non può essere spenta.");
        AddIT(68, "Stocco di Mezzanotte", "Stocco forgiato nell'oscurità assoluta. Colpisce con il silenzio della morte.");
        AddIT(69, "Stocco Invernale", "Stocco scolpito nel ghiaccio eterno. Il suo freddo può essere eguagliato solo dalla concentrazione di Mist in combattimento.");
        AddIT(70, "Stocco Indomabile", "Lo stocco leggendario del più grande duellante di Astrid. Mist guadagnò questo titolo con innumerevoli vittorie.");
        
        // Nachia Weapons (71-88)
        AddIT(71, "Bastone di Germoglio", "Ramo giovane dalla foresta degli spiriti. Anche i germogli rispondono alla chiamata di Nachia.");
        AddIT(72, "Evocatore di Spiriti", "Bastone scolpito per guidare le voci degli spiriti della foresta. Sussurrano segreti a Nachia.");
        AddIT(73, "Ramo della Foresta", "Ramo vivente che cresce ancora. La magia della foresta scorre attraverso di esso.");
        AddIT(74, "Bastone Selvaggio", "Selvaggio e imprevedibile, come Nachia stessa. Gli spiriti amano la sua energia caotica.");
        AddIT(75, "Bastone del Guardiano", "Portato da coloro che proteggono la foresta. Nachia nacque con questa responsabilità.");
        AddIT(76, "Bacchetta del Mondo Spirituale", "Bacchetta che collega il mondo materiale e quello spirituale. Nachia cammina tra i due.");
        AddIT(77, "Bastone di Legno Gelido", "Bastone congelato nell'inverno eterno. Gli spiriti freddi del nord rispondono alla sua chiamata.");
        AddIT(78, "Bastone di Radice d'Ombra", "Cresce nelle parti più profonde della foresta. Gli spiriti d'ombra danzano intorno.");
        AddIT(79, "Bastone di Legno di Brace", "Bastone che non smette mai di bruciare. Gli spiriti del fuoco sono attratti dal suo calore.");
        AddIT(80, "Bastone della Benedizione Solare", "Benedetto dagli spiriti dell'alba. La sua luce guida le anime perdute a casa.");
        AddIT(81, "Zanne di Fenrir", "Bastone con zanne di lupo spirituale in cima. Il compagno fedele di Nachia guida il suo potere.");
        AddIT(82, "Bastone di Vecchia Quercia", "Scolpito da una quercia millenaria. Gli spiriti più antichi ricordano quando fu piantata.");
        AddIT(83, "Ramo dell'Albero del Mondo", "Ramo dal leggendario Albero del Mondo. Tutti gli spiriti della foresta si inchinano ad esso.");
        AddIT(84, "Bastone del Re degli Spiriti", "Il bastone del Re degli Spiriti stesso. Nachia lo guadagnò proteggendo la foresta.");
        AddIT(85, "Bastone del Gelo Eterno", "Bastone di ghiaccio eterno dal cuore ghiacciato della foresta. Gli spiriti invernali obbediscono ai suoi ordini.");
        AddIT(86, "Posatoio della Fenice", "Bastone dove gli spiriti del fuoco nidificano. Le sue fiamme portano rinascita, non distruzione.");
        AddIT(87, "Bastone del Boschetto Raggiante", "Bastone che emana la luce di mille lucciole. Gli spiriti della speranza danzano dentro.");
        AddIT(88, "Bastone del Guardiano del Vuoto", "Bastone che protegge i confini del mondo. Gli spiriti d'ombra proteggono il suo portatore.");
        
        // Shell Weapons (89-106)
        AddIT(89, "Katana del Novizio", "Katana samurai di base per coloro che imparano la via della spada. Shell lo padroneggiò in pochi giorni.");
        AddIT(90, "Katana del Silenzio", "Katana samurai progettato per non fare rumore. Le vittime di Shell non sentono mai la morte arrivare.");
        AddIT(91, "Katana dell'Assassino", "Stretto e preciso. Shell ha bisogno solo di un colpo nel posto giusto.");
        AddIT(92, "Katana Assassino", "Efficiente. Pratico. Spietato. Come Shell stesso.");
        AddIT(93, "Katana del Cacciatore", "Katana samurai per la caccia. Shell non si ferma finché il bersaglio non è eliminato.");
        AddIT(94, "Katana del Vuoto", "Chiamato per il vuoto che Shell sente. O non sente.");
        AddIT(95, "Katana dell'Esecutore", "Katana samurai che termina innumerevoli vite. Shell non sente nulla per nessuna di esse.");
        AddIT(96, "Katana della Precisione", "Katana samurai sintonizzato per sfruttare le debolezze. Shell calcola l'attacco ottimale.");
        AddIT(97, "Katana Morso del Gelo", "Katana samurai di malizia fredda. Il suo freddo corrisponde al cuore vuoto di Shell.");
        AddIT(98, "Katana della Purificazione", "Katana samurai luminoso per lavoro oscuro. Shell non sente l'ironia.");
        AddIT(99, "Katana dell'Ombra", "Katana samurai che beve le ombre. Shell si muove invisibilmente, colpisce silenziosamente.");
        AddIT(100, "Pugnale Ardente", "Katana samurai riscaldato dal fuoco interiore. I bersagli di Shell bruciano prima di sanguinare.");
        AddIT(101, "Strumento dell'Assassino Perfetto", "Lo strumento definitivo dell'assassino definitivo. Shell è nato per questa spada.");
        AddIT(102, "Lama Spietata", "Katana samurai che non si smussa mai, brandito da un inseguitore instancabile.");
        AddIT(103, "Assassino dell'Inferno", "Katana samurai forgiato nel fuoco infernale. Brucia con l'intensità dell'unico scopo di Shell.");
        AddIT(104, "Katana del Vuoto", "Katana samurai di oscurità assoluta. Come Shell, esiste solo per terminare.");
        AddIT(105, "Katana Congelato", "Katana samurai di ghiaccio eterno. Il suo giudizio è freddo e finale come Shell.");
        AddIT(106, "Fine Raggiante", "La forma definitiva del katana samurai luminoso. Shell esegue giudizi in nome di El.");
        
        // Vesper Weapons (107-124)
        AddIT(107, "Scettro del Novizio", "Scettro semplice per i nuovi membri dell'Ordine della Fiamma Solare. Vesper iniziò il suo viaggio qui.");
        AddIT(108, "Martello del Giudizio", "Martello da guerra usato per eseguire la volontà di El. Ogni colpo è un giudizio sacro.");
        AddIT(109, "Bastone del Guardiano del Fuoco", "Arma portata da coloro che proteggono il fuoco sacro. I suoi colpi purificano l'impurità.");
        AddIT(110, "Scudo dell'Acolito", "Scudo semplice per gli acoliti dell'Ordine. La fede è la migliore difesa.");
        AddIT(111, "Scudo dell'Inquisitore", "Scudo portato dagli inquisitori. Ha visto la caduta di innumerevoli eretici.");
        AddIT(112, "Difensore del Crepuscolo", "Scudo che protegge tra luce e oscurità.");
        AddIT(113, "Martello del Fanatico", "Martello da guerra che brucia con fanatismo religioso. I nemici tremano davanti ad esso.");
        AddIT(114, "Martello Pesante della Purificazione", "Enorme martello da guerra usato per purificare il male. Un colpo frantuma il peccato.");
        AddIT(115, "Giudizio di El", "Martello da guerra benedetto personalmente da El. Il suo giudizio è inappellabile.");
        AddIT(116, "Bastione della Fede", "Scudo indistruttibile. Finché la fede non si spegne, non si romperà.");
        AddIT(117, "Scudo Sacro della Fiamma", "Scudo circondato da fuoco sacro. Il male che lo tocca viene bruciato.");
        AddIT(118, "Bastione dell'Inquisizione", "Scudo di un alto inquisitore. Vesper lo guadagnò con servizio spietato.");
        AddIT(119, "Spezzatore dell'Alba", "Martello da guerra leggendario che frantuma l'oscurità. Vesper brandisce l'incarnazione della rabbia di El.");
        AddIT(120, "Distruttore Sacro", "Martello da guerra benedetto dal sommo sacerdote. I suoi colpi portano il peso della rabbia sacra.");
        AddIT(121, "Mano Destra di El", "L'arma più sacra dell'Ordine. Vesper è lo strumento scelto della volontà di El.");
        AddIT(122, "Fortezza della Fiamma", "La difesa definitiva dell'Ordine. La luce di El protegge Vesper da ogni danno.");
        AddIT(123, "Vigilanza Eterna", "Scudo incrollabile. Come Vesper, veglia sempre sull'oscurità.");
        AddIT(124, "Giuramento del Paladino del Crepuscolo", "Scudo del leader dell'Ordine. Vesper giurò di legare la sua anima a El.");
        
        // Yubar Weapons (125-142)
        AddIT(125, "Stella Neonata", "Una stella appena formata. Yubar ricorda quando tutte le stelle erano così piccole.");
        AddIT(126, "Brace Cosmica", "Frammento di fuoco stellare. Brucia con il calore della creazione stessa.");
        AddIT(127, "Sfera di Polvere Stellare", "Polvere stellare compressa in attesa di essere modellata. Yubar tesse galassie con tale materia.");
        AddIT(128, "Catalizzatore del Sogno", "Sfera che amplifica l'energia dei sogni. Proveniente dai primi esperimenti di Yubar.");
        AddIT(129, "Nucleo della Nebulosa", "Il cuore di una nebulosa lontana. Yubar lo colse dall'arazzo cosmico.");
        AddIT(130, "Seme Celeste", "Seme che diventerà una stella. Yubar coltiva innumerevoli semi così.");
        AddIT(131, "Frammento di Supernova", "Frammento di una stella esplosa. Yubar assistette alla sua morte e preservò il suo potere.");
        AddIT(132, "Pozzo Gravitazionale", "Sfera che comprime la gravità. Lo spazio stesso si curva intorno ad essa.");
        AddIT(133, "Singolarità del Vuoto", "Punto di densità infinita. Yubar lo usa per creare e distruggere mondi.");
        AddIT(134, "Corona Solare", "Frammento dell'atmosfera esterna del sole. Brucia con la rabbia della stella.");
        AddIT(135, "Cometa Congelata", "Nucleo di cometa catturato. Porta segreti dalle profondità del cosmo.");
        AddIT(136, "Fucina Stellare", "Miniatura di un nucleo stellare. Yubar la usa per forgiare una nuova realtà.");
        AddIT(137, "Reliquia del Big Bang", "Frammento del Big Bang della creazione. Contiene l'origine dell'universo.");
        AddIT(138, "Telaio Cosmico", "Strumento che tesse la realtà. Yubar lo usa per rimodellare il destino.");
        AddIT(139, "Creazione Raggiante", "La luce della creazione stessa. Yubar la usò per dare alla luce le prime stelle.");
        AddIT(140, "Cuore dell'Entropia", "L'essenza del decadimento cosmico. Tutto finisce, e Yubar decide quando.");
        AddIT(141, "Zero Assoluto", "Il punto più freddo dell'esistenza. Anche il tempo si congela davanti ad esso.");
        AddIT(142, "Nucleo della Catastrofe", "Il cuore della distruzione cosmica. Yubar lo rilascia solo in momenti critici.");
        
        // Rings (143-162)
        AddIT(143, "Anello del Sognatore", "Anello semplice portato da coloro che entrano appena nel mondo dei sogni. Emana debole speranza.");
        AddIT(144, "Anello di Polvere Stellare", "Forgiato da polvere stellare condensata. I sognatori dicono che brilla più forte nel pericolo.");
        AddIT(145, "Segno del Sonno", "Incisione di un sonno pacifico. I portatori si riprendono più velocemente dalle ferite.");
        AddIT(146, "Anello dello Spirito", "Anello che cattura spiriti erranti. La loro luce guida i sentieri attraverso sogni oscuri.");
        AddIT(147, "Anello del Vagabondo", "Portato dai vagabondi dei regni dei sogni. Ha visto innumerevoli sentieri dimenticati.");
        AddIT(148, "Anello del Frammento d'Incubo", "Anello contenente frammenti di incubi sconfitti. La sua oscurità rafforza i coraggiosi.");
        AddIT(149, "Guardiano della Memoria", "Anello che preserva ricordi preziosi. Smoothie vende simili nel suo negozio.");
        AddIT(150, "Anello dell'Arcanum Lunare", "Anello dalla Società Arcanum Lunare. Aurena ne portò uno simile prima del suo esilio.");
        AddIT(151, "Segno della Foresta Spirituale", "Anello benedetto dagli spiriti della foresta di Nachia. La forza della natura scorre attraverso di esso.");
        AddIT(152, "Distintivo della Guardia Reale", "Anello che Lacerta portò durante il servizio. Porta ancora il peso della responsabilità.");
        AddIT(153, "Gloria del Duellante", "Anello passato tra duellanti nobili. La famiglia di Mist apprezza tali anelli.");
        AddIT(154, "Amuleto della Brace della Fiamma", "Scintilla sigillata del fuoco sacro. L'Ordine di Vesper custodisce queste reliquie.");
        AddIT(155, "Frammento del Libro di Incantesimi", "Anello fatto di magia cristallina di Bismuth. Risona con energia arcanum.");
        AddIT(156, "Segno del Cacciatore", "Anello che traccia la preda. I controllori di Shell li usano per monitorare i loro strumenti.");
        AddIT(157, "Benedizione di El", "Anello benedetto da El stesso. La sua luce dissipa gli incubi e guarisce i feriti.");
        AddIT(158, "Segno del Signore degli Incubi", "Preso da un signore degli incubi sconfitto. Il suo potere oscuro corrompe e rafforza.");
        AddIT(159, "Eredità di Astrid", "Anello dalla casa nobile di Astrid. Gli antenati di Mist lo portarono in innumerevoli duelli.");
        AddIT(160, "Occhio del Veggente", "Anello capace di vedere attraverso il velo dei sogni. Passato, presente e futuro si mescolano.");
        AddIT(161, "Anello del Sogno Primordiale", "Forgiato all'alba dei sogni. Contiene l'essenza del primo sognatore.");
        AddIT(162, "Sonno Eterno", "Anello che tocca il sonno più profondo. I portatori non temono né la morte né il risveglio.");
        
        // Amulets (163-183)
        AddIT(163, "Pendente del Sognatore", "Pendente semplice portato da coloro che entrano appena nel mondo dei sogni. Emana debole speranza.");
        AddIT(164, "Pendente del Tocco di Fuoco", "Pendente toccato dal fuoco. Il suo calore dissipa freddo e paura.");
        AddIT(165, "Lacrima Scarlatta", "Rubino a forma di lacrima di sangue. Il suo potere viene dal sacrificio.");
        AddIT(166, "Amuleto d'Ambra", "Ambra contenente una creatura antica. I suoi ricordi rafforzano il portatore.");
        AddIT(167, "Medaglia del Crepuscolo", "Medaglia che lampeggia tra luce e oscurità. I nuovi membri di Vesper portano queste.");
        AddIT(168, "Collana di Polvere Stellare", "Collana cosparsa di stelle cristalline. Smoothie le raccoglie dai sogni caduti.");
        AddIT(169, "Occhio di Smeraldo", "Gemma verde capace di vedere attraverso le illusioni. Gli incubi non possono sfuggire al suo sguardo.");
        AddIT(170, "Cuore dello Spirito della Foresta", "Gemma contenente la benedizione degli spiriti della foresta. I guardiani di Nachia apprezzano queste.");
        AddIT(171, "Segno dell'Arcanum Lunare", "Segno dagli archivi proibiti. Aurena rischia tutto studiandoli.");
        AddIT(172, "Cristallo di Bismuth", "Frammento di magia pura cristallina. Risona con energia arcanum.");
        AddIT(173, "Amuleto della Brace della Fiamma", "Scintilla sigillata del fuoco sacro. L'Ordine di Vesper custodisce queste reliquie.");
        AddIT(174, "Pendente della Zanna d'Incubo", "Zanna da un incubo potente, ora un trofeo. Shell ne porta una come ricordo.");
        AddIT(175, "Medaglia della Guardia Reale", "Medaglia d'onore dalla Guardia Reale. Lacerta ne guadagnò molte prima del suo esilio.");
        AddIT(176, "Stemma di Astrid", "Stemma nobile della famiglia di Astrid. L'eredità della famiglia di Mist pende da questa catena.");
        AddIT(177, "Lacrima Sacra di El", "Lacrima versata da El stesso. La sua luce protegge dagli incubi più oscuri.");
        AddIT(178, "Smeraldo Primordiale", "Gemma dalla prima foresta. Contiene i sogni degli spiriti antichi.");
        AddIT(179, "Occhio del Signore degli Incubi", "Occhio di un signore degli incubi sconfitto. Vede tutte le paure e le sfrutta.");
        AddIT(180, "Cuore della Fiamma", "Il nucleo del fuoco sacro stesso. Solo i più devoti possono portarlo.");
        AddIT(181, "Oracolo del Veggente", "Amuleto capace di vedere tutti i futuri possibili. La realtà si piega alle sue profezie.");
        AddIT(182, "Bussola del Viandante del Vuoto", "Amuleto che punta al vuoto. Coloro che lo seguono non tornano mai nello stesso modo.");
        AddIT(183, "Amuleto di Mida", "Amuleto leggendario toccato dal sogno dell'oro. I nemici sconfitti lasciano cadere oro aggiuntivo.");
        AddIT(478, "Raccoglitore di Polvere Stellare", "Amuleto leggendario che raccoglie polvere stellare cristallizzata dai nemici sconfitti. Ogni eliminazione produce essenza del sogno preziosa (base 1-5, scala con livello di miglioramento e forza del mostro).");
        
        // Belts (184-203)
        AddIT(184, "Cintura del Vagabondo", "Cintura di stoffa semplice portata da coloro che vagano tra i sogni. Ancoraggio l'anima.");
        AddIT(185, "Corda del Sognatore", "Corda tessuta da fili di sogno. Pulsa dolcemente ad ogni battito del cuore.");
        AddIT(186, "Fibbia di Polvere Stellare", "Cintura decorata con polvere stellare cristallina. Smoothie vende accessori simili.");
        AddIT(187, "Cintura della Sentinella", "Cintura solida portata dai guardiani dei sogni. Non si allenta mai, non fallisce mai.");
        AddIT(188, "Cintura Mistica", "Cintura infusa di magia debole. Punge quando gli incubi si avvicinano.");
        AddIT(189, "Cintura del Cacciatore", "Cintura di cuoio con borse di rifornimenti. Essenziale per lunghi viaggi nei sogni.");
        AddIT(190, "Cintura del Pellegrino", "Portata da coloro che cercano la luce di El. Offre conforto nei sogni più oscuri.");
        AddIT(191, "Cintura del Cacciatore d'Incubi", "Cintura fatta di incubi sconfitti. La loro essenza rafforza il portatore.");
        AddIT(192, "Cintura Arcanum", "Cintura dalla Società Arcanum Lunare. Amplifica l'energia magica.");
        AddIT(193, "Cintura della Fiamma", "Cintura benedetta dall'Ordine di Vesper. Brucia con fuoco protettivo.");
        AddIT(194, "Cintura della Spada del Duellante", "Cintura elegante portata dai guerrieri nobili di Astrid. Porta spade e onore.");
        AddIT(195, "Corda del Tessitore di Spiriti", "Cintura tessuta dagli spiriti della foresta per Nachia. Risona con energia naturale.");
        AddIT(196, "Cintura degli Strumenti dell'Assassino", "Cintura con compartimenti nascosti. I controllori di Shell li usano per equipaggiare i loro strumenti.");
        AddIT(197, "Cintura della Fondina del Pistoliere", "Cintura progettata per le armi da fuoco di Lacerta. Estrazione rapida, uccisione più veloce.");
        AddIT(198, "Cintura Sacra di El", "Cintura benedetta da El stesso. La sua luce ancora l'anima contro tutta l'oscurità.");
        AddIT(199, "Catena del Signore degli Incubi", "Cintura forgiata dalle catene di un signore degli incubi. Lega la paura alla volontà del portatore.");
        AddIT(200, "Cintura del Sogno Primordiale", "Cintura dal primo sogno. Contiene l'eco della creazione stessa.");
        AddIT(201, "Cintura della Radice dell'Albero del Mondo", "Cintura che cresce dalle radici dell'Albero del Mondo. La foresta di Nachia benedice la sua creazione.");
        AddIT(202, "Eredità di Astrid", "Cintura di famiglia della famiglia di Astrid. Il sangue di Mist è tessuto nel suo tessuto.");
        AddIT(203, "Cintura dell'Inquisitore del Crepuscolo", "Cintura del livello più alto di Vesper. Giudica tutti coloro che stanno davanti ad essa.");
        
        // Armor Sets - Bone Sentinel (204-215)
        AddIT(204, "Maschera della Sentinella d'Ossa", "Maschera scolpita dal cranio di un incubo caduto. I suoi occhi vuoti vedono attraverso le illusioni.");
        AddIT(205, "Elmo del Guardiano del Mondo Spirituale", "Elmo infuso con spiriti della foresta di Nachia. Sussurri di protezione circondano il portatore.");
        AddIT(206, "Corona del Signore degli Incubi", "Corona forgiata da incubi conquistati. Il portatore domina la paura stessa.");
        AddIT(207, "Gilet della Sentinella d'Ossa", "Armatura dalle costole di creature morte nei sogni. La loro protezione dura oltre la morte.");
        AddIT(208, "Corazza del Guardiano del Mondo Spirituale", "Corazza tessuta da sogni cristallizzati. Si muove e si adatta ai colpi in arrivo.");
        AddIT(209, "Corazza del Signore degli Incubi", "Armatura che pulsa con energia oscura. Gli incubi si inchinano davanti al suo portatore.");
        AddIT(210, "Schinieri della Sentinella d'Ossa", "Schinieri rinforzati con ossa di incubo. Non si stancano mai, non cedono mai.");
        AddIT(211, "Schinieri del Guardiano del Mondo Spirituale", "Schinieri benedetti dagli spiriti della foresta. Il portatore si muove come il vento attraverso le foglie.");
        AddIT(212, "Schinieri del Signore degli Incubi", "Schinieri che bevono le ombre. L'oscurità rafforza ogni passo.");
        AddIT(213, "Stivali della Sentinella d'Ossa", "Stivali per camminare silenziosamente nei regni dei sogni. I morti non fanno rumore.");
        AddIT(214, "Stivali di Ferro del Guardiano del Mondo Spirituale", "Stivali che non lasciano traccia nei regni dei sogni. Perfetti per i cacciatori di incubi.");
        AddIT(215, "Stivali del Signore degli Incubi", "Stivali che attraversano i mondi. La realtà si piega sotto ogni passo.");
        
        // Aurena Armor (216-243)
        AddIT(216, "Cappello dell'Apprendista di Aurena", "Cappello semplice portato dai novizi dell'Arcanum Lunare.");
        AddIT(217, "Turbante Mistico di Aurena", "Turbante infuso di debole energia lunare.");
        AddIT(218, "Corona Ammaliante di Aurena", "Corona che migliora l'affinità magica del portatore.");
        AddIT(219, "Corona Antica di Aurena", "Reliquia antica tramandata attraverso le generazioni.");
        AddIT(220, "Fascia del Falco di Aurena", "Fascia benedetta dagli spiriti del cielo.");
        AddIT(221, "Corona del Potere di Aurena", "Corona potente che emana potere arcanum.");
        AddIT(222, "Corona Lunare di Aurena", "Corona leggendaria di un maestro dell'Arcanum Lunare.");
        AddIT(223, "Veste Blu di Aurena", "Veste semplice portata dai maghi lunari.");
        AddIT(224, "Veste del Mago di Aurena", "Veste apprezzata dai maghi praticanti.");
        AddIT(225, "Veste Tessuta con Incantesimi di Aurena", "Veste tessuta con fili magici.");
        AddIT(226, "Veste Illuminante di Aurena", "Veste che guida la saggezza lunare.");
        AddIT(227, "Sudario del Sogno di Aurena", "Sudario tessuto da sogni cristallizzati.");
        AddIT(228, "Veste Reale delle Squame di Aurena", "Veste adatta alla famiglia reale lunare.");
        AddIT(229, "Veste della Regina del Ghiaccio di Aurena", "Veste leggendaria della Regina del Ghiaccio stessa.");
        AddIT(230, "Schinieri Blu di Aurena", "Schinieri semplici portati dai maghi aspiranti.");
        AddIT(231, "Gonna di Fibra di Aurena", "Gonna leggera che permette movimento libero.");
        AddIT(232, "Schinieri Elfici di Aurena", "Schinieri eleganti progettati dagli elfi.");
        AddIT(233, "Schinieri Illuminanti di Aurena", "Schinieri benedetti dalla saggezza.");
        AddIT(234, "Gonna Glaciale di Aurena", "Gonna infusa di magia del gelo.");
        AddIT(235, "Culottes del Gelo di Aurena", "Culottes che emettono forza del freddo.");
        AddIT(236, "Schinieri Magnifici di Aurena", "Schinieri leggendari di eleganza incomparabile.");
        AddIT(237, "Pantofole Soffici Lunari", "Scarpe soffici portate dagli acoliti dell'Arcanum Lunare. Ammortizzano ogni passo con luce lunare.");
        AddIT(238, "Sandali del Pellegrino", "Sandali semplici benedetti dagli anziani della Società. Aurena ricorda ancora il loro calore.");
        AddIT(239, "Scarpe della Mistica Orientale", "Scarpe esotiche da terre lontane. Guidano energia curativa attraverso la terra.");
        AddIT(240, "Grazia del Viandante del Sogno", "Stivali che camminano tra il mondo sveglio e quello dei sogni. Perfetti per i guaritori che cercano anime perdute.");
        AddIT(241, "Stivali del Viandante dell'Anima", "Stivali infusi con l'essenza di guaritori defunti. La loro saggezza guida ogni passo.");
        AddIT(242, "Stivali dell'Inseguitore dell'Anima", "Stivali oscuri che inseguono i feriti. Aurena li usa per trovare coloro che hanno bisogno di aiuto... o quelli che l'hanno ferita.");
        AddIT(243, "Ali del Saggio Caduto", "Stivali leggendari che si dice concedano la capacità di volare ai cuori puri. L'esilio di Aurena provò la sua qualificazione.");
        
        // Bismuth Armor (244-271)
        AddIT(244, "Cappello Conico di Bismuth", "Cappello semplice per apprendisti cristallini.");
        AddIT(245, "Cappello di Smeraldo di Bismuth", "Cappello intarsiato con cristalli di smeraldo.");
        AddIT(246, "Maschera Glaciale di Bismuth", "Maschera fatta di cristalli ghiacciati.");
        AddIT(247, "Maschera Alarharim di Bismuth", "Maschera antica di potere cristallino.");
        AddIT(248, "Elmo Prismatico di Bismuth", "Elmo che rifrange la luce in arcobaleno.");
        AddIT(249, "Corona Riflettente di Bismuth", "Corona che riflette energia magica.");
        AddIT(250, "Corona Incandescente di Bismuth", "Corona leggendaria che brucia con fuoco cristallino.");
        AddIT(251, "Veste di Ghiaccio di Bismuth", "Veste raffreddata dalla magia cristallina.");
        AddIT(252, "Veste del Monaco di Bismuth", "Veste semplice per meditazione.");
        AddIT(253, "Veste Glaciale di Bismuth", "Veste congelata nel gelo eterno.");
        AddIT(254, "Armatura Cristallina di Bismuth", "Armatura formata da cristalli viventi.");
        AddIT(255, "Armatura Prismatica di Bismuth", "Armatura che piega la luce stessa.");
        AddIT(256, "Armatura a Piastre Congelata di Bismuth", "Armatura a piastre del gelo eterno.");
        AddIT(257, "Armatura a Piastre Sacra di Bismuth", "Armatura leggendaria benedetta dagli dei cristallini.");
        AddIT(258, "Schinieri di Maglia di Bismuth", "Schinieri base con anelli cristallini.");
        AddIT(259, "Schinieri di Fibra di Bismuth", "Schinieri leggeri per maghi cristallini.");
        AddIT(260, "Schinieri di Smeraldo di Bismuth", "Schinieri decorati con cristalli di smeraldo.");
        AddIT(261, "Pantaloni Strani di Bismuth", "Pantaloni che pulsano con energia strana.");
        AddIT(262, "Schinieri Prismatici di Bismuth", "Schinieri che brillano di luce prismatica.");
        AddIT(263, "Schinieri Alarharim di Bismuth", "Schinieri antichi di Alarharim.");
        AddIT(264, "Schinieri del Falco di Bismuth", "Schinieri leggendari dell'Ordine del Falco.");
        AddIT(265, "Pantofole dello Studioso", "Scarpe comode per lavoro lungo nelle biblioteche. La conoscenza scorre attraverso le loro suole.");
        AddIT(266, "Bende Alarharim", "Bende antiche da una civiltà perduta. Sussurrano incantesimi dimenticati.");
        AddIT(267, "Scarpe di Ghiaccio", "Stivali scolpiti dal ghiaccio eterno. Bismuth lascia modelli di gelo dove cammina.");
        AddIT(268, "Passo del Fiore di Gelo", "Stivali eleganti che fioriscono con cristalli di ghiaccio. Ogni passo crea un giardino di gelo.");
        AddIT(269, "Stivali della Risonanza Cristallina", "Stivali che amplificano l'energia magica. I cristalli risuonano con potere inutilizzato.");
        AddIT(270, "Stivali Arcanum Prismatici", "Stivali che rifrangono la luce in pura energia magica. La realtà si piega intorno ad ogni passo.");
        AddIT(271, "Stivali del Viandante del Vuoto", "Stivali leggendari che attraversano il vuoto stesso. Bismuth vede sentieri che altri non possono immaginare.");
        
        // Lacerta Armor (272-299)
        AddIT(272, "Elmo di Pelle di Lizardman", "Elmo base per guerrieri drago.");
        AddIT(273, "Elmo di Maglia di Lizardman", "Elmo di maglia solido.");
        AddIT(274, "Elmo del Guerriero di Lizardman", "Elmo portato da guerrieri esperti.");
        AddIT(275, "Elmo delle Squame di Drago di Lizardman", "Elmo forgiato da squame di drago.");
        AddIT(276, "Elmo Reale di Lizardman", "Elmo adatto alla famiglia reale drago.");
        AddIT(277, "Elmo d'Élite Drago-Uomo di Lizardman", "Elmo d'élite dell'Ordine Drago-Uomo.");
        AddIT(278, "Elmo d'Oro di Lizardman", "Elmo d'oro leggendario del Re Drago.");
        AddIT(279, "Armatura di Pelle di Lizardman", "Armatura base per guerrieri drago.");
        AddIT(280, "Armatura delle Squame di Lizardman", "Armatura rinforzata con squame.");
        AddIT(281, "Armatura del Cavaliere di Lizardman", "Armatura pesante per cavalieri drago.");
        AddIT(282, "Maglia delle Squame di Drago di Lizardman", "Maglia forgiata da squame di drago.");
        AddIT(283, "Maglia Reale Drago-Uomo di Lizardman", "Maglia reale dell'Ordine Drago-Uomo.");
        AddIT(284, "Armatura a Piastre del Falco di Lizardman", "Armatura a piastre dell'Ordine del Falco.");
        AddIT(285, "Armatura d'Oro di Lizardman", "Armatura d'oro leggendaria del Re Drago.");
        AddIT(286, "Schinieri di Pelle di Lizardman", "Schinieri base per guerrieri drago.");
        AddIT(287, "Schinieri Rivettati di Lizardman", "Schinieri rinforzati con rivetti.");
        AddIT(288, "Schinieri del Cavaliere di Lizardman", "Schinieri pesanti per cavalieri drago.");
        AddIT(289, "Schinieri delle Squame di Drago di Lizardman", "Schinieri forgiati da squame di drago.");
        AddIT(290, "Schinieri della Corona di Lizardman", "Schinieri adatti alla famiglia reale.");
        AddIT(291, "Schinieri Magnifici di Lizardman", "Schinieri magnifici di artigianato maestro.");
        AddIT(292, "Schinieri d'Oro di Lizardman", "Schinieri d'oro leggendari del Re Drago.");
        AddIT(293, "Stivali di Pelle Consumati", "Stivali semplici che hanno visto molte battaglie. Portano l'aroma dell'avventura.");
        AddIT(294, "Stivali da Combattimento Rattoppati", "Stivali riparati innumerevoli volte. Ogni toppa racconta una storia di sopravvivenza.");
        AddIT(295, "Stivali del Guerriero d'Acciaio", "Stivali pesanti forgiati per il combattimento. Ancorano Lacerta al suolo durante la battaglia.");
        AddIT(296, "Schinieri del Guardiano", "Stivali portati da guardiani d'élite. Non retrocedono mai, non si arrendono mai.");
        AddIT(297, "Schinieri delle Squame di Drago", "Stivali fatti di squame di drago. Concedono al portatore la tenacia del drago.");
        AddIT(298, "Stivali del Signore della Guerra Drago-Uomo", "Stivali di antichi generali drago-uomo. Temibili su qualsiasi campo di battaglia.");
        AddIT(299, "Stivali del Campione d'Oro", "Stivali leggendari del più grande guerriero di un'era. Il destino di Lacerta attende.");
        
        // Mist Armor (300-327)
        AddIT(300, "Turbante di Mist", "Turbante semplice che nasconde l'identità.");
        AddIT(301, "Turbante Leggero di Mist", "Turbante leggero e traspirante.");
        AddIT(302, "Turbante della Visione Notturna di Mist", "Turbante che migliora la visione notturna.");
        AddIT(303, "Fascia del Fulmine di Mist", "Fascia carica di energia elettrica.");
        AddIT(304, "Cappuccio del Cobra di Mist", "Cappuccio mortale come un serpente.");
        AddIT(305, "Maschera dell'Inseguitore dell'Inferno di Mist", "Maschera che insegue la preda.");
        AddIT(306, "Sussurratore Oscuro di Mist", "Maschera leggendaria che sussurra morte.");
        AddIT(307, "Briglia di Pelle di Mist", "Briglia leggera per agilità.");
        AddIT(308, "Mantello del Ranger di Mist", "Mantello per movimento rapido.");
        AddIT(309, "Tunica di Mezzanotte di Mist", "Tunica per colpi notturni.");
        AddIT(310, "Tunica del Fulmine di Mist", "Tunica carica di fulmini.");
        AddIT(311, "Armatura della Tensione di Mist", "Armatura che pulsa con energia elettrica.");
        AddIT(312, "Armatura del Maestro Arciere di Mist", "Armatura di un maestro arciere.");
        AddIT(313, "Mantello del Signore Oscuro di Mist", "Mantello leggendario del Signore Oscuro.");
        AddIT(314, "Schinieri del Ranger di Mist", "Schinieri portati dai ranger.");
        AddIT(315, "Schinieri del Sopravvissuto della Giungla di Mist", "Schinieri del sopravvissuto della giungla.");
        AddIT(316, "Sarong di Mezzanotte di Mist", "Sarong per operazioni notturne.");
        AddIT(317, "Schinieri del Fulmine di Mist", "Schinieri carichi di fulmini.");
        AddIT(318, "Schinieri della Cavalletta di Mist", "Schinieri per salti sorprendenti.");
        AddIT(319, "Schinieri del Demone Verde di Mist", "Schinieri della velocità demoniaca.");
        AddIT(320, "Schinieri del Viandante dell'Anima di Mist", "Schinieri leggendari che attraversano i mondi.");
        AddIT(321, "Stivali da Ricognizione Temporanei", "Stivali assemblati da frammenti. Leggeri e silenziosi, perfetti per la ricognizione.");
        AddIT(322, "Stivali del Corridore della Palude", "Stivali impermeabili per terreno difficile. Mist conosce ogni scorciatoia.");
        AddIT(323, "Viandante Veloce", "Stivali incantati che accelerano i passi del portatore. Perfetti per tattiche hit-and-run.");
        AddIT(324, "Stivali del Cacciatore di Cervo", "Stivali che inseguono la preda.");
        AddIT(325, "Stivali del Colpitore del Fulmine", "Stivali carichi di energia del fulmine.");
        AddIT(326, "Stivali da Combattimento del Viandante del Fuoco", "Stivali capaci di camminare attraverso il fuoco.");
        AddIT(327, "Calpestatore della Sofferenza", "Stivali leggendari che calpestano tutta la sofferenza.");
        
        // Nachia Armor (328-355)
        AddIT(328, "Cappello di Pelliccia di Nachia", "Cappello semplice per cacciatori della foresta.");
        AddIT(329, "Copricapo di Piume di Nachia", "Copricapo decorato con piume.");
        AddIT(330, "Maschera dello Sciamano di Nachia", "Maschera del potere spirituale.");
        AddIT(331, "Cappuccio della Terra di Nachia", "Cappuccio benedetto dagli spiriti della terra.");
        AddIT(332, "Elmo della Natura di Nachia", "Elmo infuso di forza naturale.");
        AddIT(333, "Corona dell'Albero di Nachia", "Corona che cresce da un albero antico.");
        AddIT(334, "Corona della Foglia di Nachia", "Corona leggendaria del Guardiano della Foresta.");
        AddIT(335, "Armatura di Pelliccia di Nachia", "Armatura fatta di creature della foresta.");
        AddIT(336, "Armatura Indigena di Nachia", "Armatura tradizionale delle tribù della foresta.");
        AddIT(337, "Cappotto di Legno Verde di Nachia", "Cappotto tessuto da viti viventi.");
        AddIT(338, "Mantello della Terra di Nachia", "Mantello benedetto dagli spiriti della terra.");
        AddIT(339, "Abbraccio della Natura di Nachia", "Armatura unita con la natura.");
        AddIT(340, "Armatura del Nido della Palude di Nachia", "Armatura dalle paludi profonde.");
        AddIT(341, "Veste della Foglia di Nachia", "Veste leggendaria del Guardiano della Foresta.");
        AddIT(342, "Pantaloncini di Pelliccia di Mammut di Nachia", "Pantaloncini caldi fatti di pelliccia di mammut.");
        AddIT(343, "Pantaloncini di Pelle di Nachia", "Schinieri da caccia tradizionali.");
        AddIT(344, "Schinieri di Cervo di Nachia", "Schinieri fatti di pelle di cervo.");
        AddIT(345, "Schinieri della Terra di Nachia", "Schinieri benedetti dagli spiriti della terra.");
        AddIT(346, "Schinieri della Foglia di Nachia", "Schinieri tessuti da foglie magiche.");
        AddIT(347, "Perizoma dell'Uomo-Cinghiale di Nachia", "Perizoma da un potente uomo-cinghiale.");
        AddIT(348, "Schinieri della Caccia di Sangue", "Schinieri leggendari della caccia di sangue.");
        AddIT(349, "Stivali Foderati di Pelliccia", "Stivali caldi e comodi.");
        AddIT(350, "Stivali di Pelle di Tasso", "Stivali fatti di pelle di tasso.");
        AddIT(351, "Stivali del Colpo del Cobra", "Stivali veloci come un serpente.");
        AddIT(352, "Bende dell'Inseguitore della Foresta", "Bende silenziose nella foresta.");
        AddIT(353, "Stivali del Cacciatore della Terra", "Stivali che inseguono la preda.");
        AddIT(354, "Stivali del Ranger del Fiore della Febbre", "Stivali decorati con fiori della febbre. Concedono velocità febbrile e precisione mortale.");
        AddIT(355, "Stivali del Predatore di Sangue", "Stivali leggendari imbevuti del sangue di innumerevoli cacce. Nachia divenne il predatore supremo.");
        
        // Shell Armor (356-383)
        AddIT(356, "Elmo Danneggiato di Shell", "Elmo consumato dal mondo sotterraneo.");
        AddIT(357, "Maschera Rotta di Shell", "Maschera rotta che offre ancora protezione.");
        AddIT(358, "Elmo del Signore delle Ossa di Shell", "Elmo forgiato dai resti del Signore delle Ossa.");
        AddIT(359, "Elmo di Scheletro di Shell", "Elmo formato da ossa.");
        AddIT(360, "Elmo della Morte di Shell", "Elmo della morte stessa.");
        AddIT(361, "Guardiano di Scheletro Nokferatu di Shell", "Elmo di ossa di vampiro.");
        AddIT(362, "Elmo Demoniaco di Shell", "Elmo leggendario del Signore Demone.");
        AddIT(363, "Sudario Funerario di Shell", "Sudario dalla tomba.");
        AddIT(364, "Mantello Vecchio di Shell", "Mantello vecchio dai tempi antichi.");
        AddIT(365, "Mantello di Ossa Nokferatu di Shell", "Mantello tessuto da ossa.");
        AddIT(366, "Mantello dell'Anima di Shell", "Mantello di anime inquiete.");
        AddIT(367, "Veste della Morte di Shell", "Veste della morte.");
        AddIT(368, "Veste del Mondo Sotterraneo di Shell", "Veste dalle profondità.");
        AddIT(369, "Armatura Demoniaca di Shell", "Armatura leggendaria del Signore Demone.");
        AddIT(370, "Grembiule Danneggiato di Shell", "Grembiule danneggiato ma utile.");
        AddIT(371, "Gonna di Ossa Mutate di Shell", "Gonna fatta di ossa mutate.");
        AddIT(372, "Bende delle Spine Nokferatu di Shell", "Bende con spine.");
        AddIT(373, "Guardiano della Carne Nokferatu di Shell", "Protezione fatta di carne preservata.");
        AddIT(374, "Schinieri di Sangue di Shell", "Schinieri imbevuti di sangue.");
        AddIT(375, "Viandante del Sangue Nokferatu di Shell", "Schinieri che camminano nel sangue.");
        AddIT(376, "Schinieri Demoniaci di Shell", "Schinieri leggendari del Signore Demone.");
        AddIT(377, "Schinieri Corazzati", "Stivali metallici pesanti che schiacciano tutto sotto i piedi. La difesa è il miglior attacco.");
        AddIT(378, "Stivali del Navigatore", "Stivali solidi che resistono a qualsiasi tempesta. Ancorano Shell nelle correnti della battaglia.");
        AddIT(379, "Schinieri degli Abissi", "Stivali forgiati nelle profondità dell'oceano. Resistenti a qualsiasi pressione.");
        AddIT(380, "Stivali Schiaccia-Ossa", "Stivali rinforzati con ossa di mostri. Ogni passo è una minaccia.");
        AddIT(381, "Stivali della Fortezza di Magma", "Stivali forgiati nel fuoco del vulcano. Emettono calore che brucia gli attaccanti.");
        AddIT(382, "Stivali del Calpestatore di Sangue", "Stivali brutali che lasciano tracce di distruzione. I nemici temono di combatterli.");
        AddIT(383, "Stivali del Guardiano Incrollabile", "Stivali leggendari di un difensore indistruttibile. Shell diventa una fortezza immobile.");
        
        // Vesper Armor (384-411)
        AddIT(384, "Cappello della Strega di Vesper", "Cappello semplice per tessitori di sogni.");
        AddIT(385, "Cappuccio Strano di Vesper", "Cappuccio che sussurra segreti.");
        AddIT(386, "Cappuccio Strano di Vesper", "Cappuccio che pulsa con energia strana.");
        AddIT(387, "Cappuccio dell'Energia dell'Incubo di Vesper", "Cappuccio dell'energia dell'incubo.");
        AddIT(388, "Bende della Disperazione di Vesper", "Bende che si nutrono di disperazione.");
        AddIT(389, "Corona del Mago Oscuro di Vesper", "Corona della magia oscura.");
        AddIT(390, "Cappello di Ferunbras di Vesper", "Cappello leggendario del Maestro dell'Oscurità.");
        AddIT(391, "Veste Rossa di Vesper", "Veste per maghi del sogno aspiranti.");
        AddIT(392, "Legami dell'Anima di Vesper", "Legami che guidano le anime.");
        AddIT(393, "Veste dell'Energia di Vesper", "Veste che crepita con energia.");
        AddIT(394, "Gonna dello Spirito di Vesper", "Gonna tessuta da energia spirituale.");
        AddIT(395, "Mantello dell'Anima di Vesper", "Mantello che contiene anime.");
        AddIT(396, "Bende dell'Anima di Vesper", "Bende di anime catturate.");
        AddIT(397, "Veste del Drago Arcanum di Vesper", "Veste leggendaria del Drago Arcanum.");
        AddIT(398, "Schinieri dell'Anima di Vesper", "Schinieri infusi con energia dell'anima.");
        AddIT(399, "Schinieri Esotici di Vesper", "Schinieri esotici da regni lontani.");
        AddIT(400, "Schinieri dell'Anima di Vesper", "Schinieri che guidano la forza dell'anima.");
        AddIT(401, "Pantaloni di Sangue di Vesper", "Pantaloni tinti con magia del sangue.");
        AddIT(402, "Schinieri di Magma di Vesper", "Schinieri forgiati nel fuoco magico.");
        AddIT(403, "Schinieri della Saggezza di Vesper", "Schinieri della saggezza antica.");
        AddIT(404, "Pantaloni degli Antichi di Vesper", "Pantaloni leggendari degli Antichi.");
        AddIT(405, "Pantofole dell'Acolito", "Scarpe soffici portate dai nuovi membri del tempio. Portano preghiere ad ogni passo.");
        AddIT(406, "Scarpe del Tempio", "Scarpe tradizionali dei servi di El. Ancorano Vesper alla sua fede.");
        AddIT(407, "Stivali del Monaco Strano", "Stivali portati da monaci che studiano testi proibiti. Conoscenza e fede si intrecciano.");
        AddIT(408, "Bende Benedette dai Nani", "Bende incantate dagli artigiani nani. La tecnologia incontra la divinità.");
        AddIT(409, "Pantofole di Seta di Vampiro", "Scarpe eleganti tessute da seta di vampiro. Estrangono la vita dalla terra stessa.");
        AddIT(410, "Pantofole dell'Assassino di Demoni", "Scarpe verdi benedette per distruggere il male. Bruciano demoni ad ogni passo.");
        AddIT(411, "Stivali dello Scacciatore d'Incubi", "Stivali leggendari che attraversano gli incubi per salvare gli innocenti. La missione definitiva di Vesper.");
        
        // Yubar Armor (412-439)
        AddIT(412, "Maschera Tribale di Yubar", "Maschera per guerrieri tribali.");
        AddIT(413, "Elmo Vichingo di Yubar", "Elmo per guerrieri del nord.");
        AddIT(414, "Elmo con Corna di Yubar", "Elmo con corna terrificanti.");
        AddIT(415, "Copricapo Ix Tenace di Yubar", "Copricapo per la tribù Ix.");
        AddIT(416, "Copricapo della Paura del Fuoco di Yubar", "Copricapo che brucia con paura.");
        AddIT(417, "Elmo Ix Tenace di Yubar", "Elmo per guerrieri Ix.");
        AddIT(418, "Volto dell'Apocalisse di Yubar", "Volto leggendario dell'Apocalisse.");
        AddIT(419, "Pelle d'Orso di Yubar", "Armatura da un orso potente.");
        AddIT(420, "Mantello di Pelliccia di Mammut di Yubar", "Mantello da un mammut.");
        AddIT(421, "Veste Ix Tenace di Yubar", "Veste per la tribù Ix.");
        AddIT(422, "Cappotto di Magma di Yubar", "Cappotto forgiato nella magma.");
        AddIT(423, "Corazza Ix Tenace di Yubar", "Corazza per guerrieri Ix.");
        AddIT(424, "Armatura a Piastre di Lava di Yubar", "Armatura a piastre di lava.");
        AddIT(425, "Armatura del Gigante del Fuoco di Yubar", "Armatura leggendaria del Gigante del Fuoco.");
        AddIT(426, "Schinieri Ix Tenaci di Yubar", "Schinieri per la tribù Ix.");
        AddIT(427, "Pantaloni di Pelle Mutata di Yubar", "Pantaloni fatti di pelle mutata.");
        AddIT(428, "Gonna Ix Tenace di Yubar", "Gonna per guerrieri Ix.");
        AddIT(429, "Schinieri Nani di Yubar", "Schinieri nani solidi.");
        AddIT(430, "Schinieri di Lega di Yubar", "Schinieri rinforzati con lega.");
        AddIT(431, "Schinieri a Piastre di Yubar", "Schinieri a piastre pesanti.");
        AddIT(432, "Schinieri Nani di Yubar", "Schinieri leggendari dell'artigianato nano.");
        AddIT(433, "Stivali da Combattimento Temporanei", "Stivali assemblati da frammenti del campo di battaglia. Yubar fa con ciò che trova.");
        AddIT(434, "Bende Tribali", "Bende tradizionali del popolo di Yubar. Lo collegano ai suoi antenati.");
        AddIT(435, "Schinieri del Guerriero Cervo", "Stivali decorati con corna di cervo. Concedono il potere del Re della Foresta.");
        AddIT(436, "Stivali del Calpestatore delle Zanne", "Stivali brutali con punte a forma di zanna. Schiacciano i nemici sotto i piedi.");
        AddIT(437, "Stivali del Berserker di Sangue", "Stivali rosso sangue che accendono la rabbia di Yubar. Il dolore diventa forza.");
        AddIT(438, "Calpestatore dell'Anima", "Stivali che raccolgono le anime dei nemici caduti. Il loro potere cresce con ogni battaglia.");
        AddIT(439, "Stivali del Campione del Ritorno a Casa", "Stivali leggendari che riportano spiritualmente Yubar a casa. I suoi antenati combattono al suo fianco.");
        
        // Legendary Items (440-443)
        AddIT(440, "Elmo Alato del Viandante del Sogno", "Elmo leggendario che concede ai viandanti del sogno la capacità di volare. Attacca automaticamente i nemici più vicini.");
        AddIT(441, "Armatura a Piastre della Magia del Sogno", "Armatura leggendaria che protegge i viandanti del sogno di tutti i regni. Mira automaticamente ai nemici più vicini.");
        AddIT(442, "Schinieri degli Abissi", "Schinieri leggendari forgiati nelle parti più profonde dei sogni.");
        AddIT(443, "Stivali del Camminare sull'Acqua", "Stivali leggendari che permettono al portatore di camminare sull'acqua. Tesoro cercato dai viandanti del sogno di tutti i regni.");
        
        // Consumables (444-447)
        AddIT(444, "Tonico del Sognatore", "Un semplice rimedio preparato con erbe del mondo sveglio. Anche il sogno più debole inizia con una goccia di speranza.");
        AddIT(445, "Essenza di Sogno", "Distillata dai ricordi di sonni tranquilli. Chi la beve sente il calore dei sogni dimenticati che lava le sue ferite.");
        AddIT(446, "Vitalità Lucida", "Creata da Smoothie usando polvere di stelle e frammenti di incubo. Il liquido brilla della luce di mille stelle addormentate.");
        AddIT(447, "Elisir di El", "Una bevanda sacra benedetta dalla luce di El stesso. L'ordine di Vesper custodisce gelosamente la sua ricetta, poiché può guarire anche le ferite inflitte dagli incubi più oscuri.");
        
        // Universal Legendary Belt (448)
        AddIT(448, "Cintura dei Sogni Infiniti", "Una cintura leggendaria che lega il portatore al ciclo eterno dei sogni. Il suo potere cresce con ogni incubo conquistato.");
        
        // Hero-Specific Legendary Belts (449-457)
        AddIT(449, "Cintura Tessitrice di Vita di Aurena", "Una cintura forgiata dalla ricerca rubata di Aurena sulla forza vitale. Tesse vita e morte in perfetto equilibrio.");
        AddIT(450, "Cintura Cristallina di Bismuth", "Una cintura di cristallo puro che pulsa con la visione della ragazza cieca. Vede ciò che gli occhi non possono.");
        AddIT(451, "Cintura dell'Occhio Cremisi di Lacerta", "Una cintura che canalizza lo sguardo leggendario di Lacerta. Nessun bersaglio sfugge al suo potere vigile.");
        AddIT(452, "Cintura del Campione Duellante di Mist", "La cintura definitiva della Casa Astrid. L'onore di Mist è intessuto in ogni filo.");
        AddIT(453, "Cintura dell'Albero del Mondo di Nachia", "Una cintura coltivata dall'Albero del Mondo stesso. La furia della natura scorre attraverso di essa.");
        AddIT(454, "Catena Infrangibile di Husk", "Una cintura forgiata da una risoluzione infrangibile. La determinazione di Husk è la sua forza.");
        AddIT(455, "Cintura dell'Assassino delle Ombre di Shell", "Una cintura di oscurità perfetta. I manipolatori di Shell non riuscirono mai a spezzare la sua volontà.");
        AddIT(456, "Cintura dell'Inquisitore del Crepuscolo di Vesper", "La cintura di rango più alto dell'Ordine di El. Giudica gli incubi e bandisce l'oscurità.");
        AddIT(457, "Cintura del Campione Ancestrale di Yubar", "Una cintura che invoca gli antenati di Yubar. La loro forza scorre attraverso ogni fibra.");
        
        // Universal Legendary Ring (458)
        AddIT(458, "Anello dei Sogni Eterni", "Un anello leggendario che collega il portatore al regno infinito dei sogni. Il suo potere trascende tutti i confini.");
        
        // Hero-Specific Legendary Rings (459-467)
        AddIT(459, "Anello del Grande Saggio di Aurena", "L'anello del Grande Saggio dell'Arcanum Lunare. Aurena lo prese come prova della loro ipocrisia.");
        AddIT(460, "Anello del Cuore di Gemma di Bismuth", "Un anello di magia cristallina pura. Pulsa con l'essenza trasformata della ragazza cieca.");
        AddIT(461, "Anello dell'Esecutore Reale di Lacerta", "L'anello dell'esecutore d'élite della Guardia Reale. Lacerta lo guadagnò attraverso innumerevoli battaglie.");
        AddIT(462, "Anello dell'Eredità Astrid di Mist", "L'anello ancestrale della Casa Astrid. Il lignaggio di Mist scorre attraverso il suo metallo.");
        AddIT(463, "Anello del Guardiano della Foresta di Nachia", "Un anello benedetto dagli spiriti della foresta di Nachia. Il potere della natura è la sua essenza.");
        AddIT(464, "Anello della Volontà Incrollabile di Husk", "Un anello forgiato da una volontà infrangibile. La determinazione di Husk è il suo potere.");
        AddIT(465, "Anello dell'Ombra Perfetta di Shell", "Un anello di oscurità assoluta. I manipolatori di Shell non riuscirono mai a controllare il suo potere.");
        AddIT(466, "Anello della Fiamma Sacra di Vesper", "Un anello contenente l'essenza pura della Fiamma Sacra di El. Bandisce tutta l'oscurità.");
        AddIT(467, "Anello del Campione Tribale di Yubar", "Un anello che canalizza la forza degli antenati di Yubar. La loro furia potenzia ogni colpo.");
        
        // Universal Legendary Amulet (468)
        AddIT(468, "Amuleto dei Sogni Infiniti", "Un amuleto leggendario che lega il portatore al ciclo eterno dei sogni. Il suo potere trascende tutti i confini.");
        
        // Hero-Specific Legendary Amulets (469-477)
        AddIT(469, "Amuleto Tessitore di Vita di Aurena", "Un amuleto forgiato dalla ricerca rubata di Aurena sulla forza vitale. Tesse vita e morte in perfetto equilibrio.");
        AddIT(470, "Cuore Cristallino di Bismuth", "Un amuleto di cristallo puro che pulsa con la visione della ragazza cieca. Vede ciò che gli occhi non possono.");
        AddIT(471, "Amuleto dell'Occhio Cremisi di Lacerta", "Un amuleto che canalizza lo sguardo leggendario di Lacerta. Nessun bersaglio sfugge al suo potere vigile.");
        AddIT(472, "Amuleto del Campione Duellante di Mist", "L'amuleto definitivo della Casa Astrid. L'onore di Mist è intessuto in ogni gemma.");
        AddIT(473, "Amuleto dell'Albero del Mondo di Nachia", "Un amuleto coltivato dall'Albero del Mondo stesso. La furia della natura scorre attraverso di esso.");
        AddIT(474, "Amuleto Infrangibile di Husk", "Un amuleto forgiato da una risoluzione infrangibile. La determinazione di Husk è la sua forza.");
        AddIT(475, "Amuleto dell'Assassino delle Ombre di Shell", "Un amuleto di oscurità perfetta. I manipolatori di Shell non riuscirono mai a spezzare la sua volontà.");
        AddIT(476, "Amuleto dell'Inquisitore del Crepuscolo di Vesper", "L'amuleto di rango più alto dell'Ordine di El. Giudica gli incubi e bandisce l'oscurità.");
        AddIT(477, "Amuleto del Campione Ancestrale di Yubar", "Un amuleto che invoca gli antenati di Yubar. La loro forza scorre attraverso ogni gemma.");
    }
    
    private static void AddIT(int id, string name, string desc)
    {
        _nameIT[id] = name;
        _descIT[id] = desc;
    }
    
    // ============================================================
    // TURKISH TRANSLATIONS
    // ============================================================
    private static void InitializeTR()
    {
        _nameTR = new Dictionary<int, string>();
        _descTR = new Dictionary<int, string>();
        
        // Aurena Silahları (1-18)
        AddTR(1, "Acemi Pençeleri", "Ay Arcanum Cemiyeti'nin yeni üyelerine verilen basit pençeler. Aurena sürgün edildikten sonra bile kendininkileri sakladı.");
        AddTR(2, "Bilgin Pençeleri", "Yasak metinlerde bilenenmiş pençeler. Bıraktıkları çizikler sırları fısıldar.");
        AddTR(3, "Ay Işığı Pençeleri", "Ay ışığı altında hafifçe parlayan pençeler. Ay enerjisini yıkıcı darbelere dönüştürürler.");
        AddTR(4, "Arcanum Usturalar", "İyileştirme runlarıyla oyulmuş pençeler - artık iyileştirmek yerine yırtmak için kullanılıyor.");
        AddTR(5, "Sürgün Pençeleri", "Sürgününden sonra gizlice dövülmüş. Her kesim, Cemiyetin ondan aldıklarını hatırlatır.");
        AddTR(6, "Kâfirin Tutuşu", "Bir zamanlar başka bir sürülmüş bilgeye ait olan pençeler. Ruhları Aurena'nın darbelerini yönlendirir.");
        AddTR(7, "Kurban Pençeleri", "Aurena'nın yaşam gücü azaldıkça daha keskin hale gelen pençeler. Cemiyet bu araştırmayı çok tehlikeli buldu.");
        AddTR(8, "Ayaza Bağlı Pençeler", "Cemiyetin en derin mahzenindeki buzla dondurulmuş pençeler. Dokunuşları eti ve ruhu uyuşturur.");
        AddTR(9, "Ay Parçalayıcıları", "Yasak büyülerle güçlendirilmiş. Açtıkları her yara Aurena'nın müttefiklerini iyileştirir.");
        AddTR(10, "Işıltılı Pençeler", "Çalınmış güneş ışığıyla kutsanmış pençeler. Aurena'nın ışık büyüsü araştırması hem güneş hem de ay kültlerini öfkelendirdi.");
        AddTR(11, "Kor Pençeleri", "İstekli rüya görenlerin yaşam gücüyle tutuşturulmuş pençeler. Alevler asla sönmez.");
        AddTR(12, "Gölge Pençeleri", "Kabus özünde batırılmış pençeler. Aurena karanlığın ışığın iyileştiremediğini iyileştirdiğini öğrendi.");
        AddTR(13, "Buzul Biçici", "Ebedi buzdan oyulmuş pençeler. Düşmanların kanını dondurur ve Aurena'nın yaşam gücünü korur.");
        AddTR(14, "Anka Kuşu Pençeleri", "Anka kuşu alevlerinde dövülmüş ve Aurena'nın kendi kanıyla sulanmış pençeler. Sönmeyen öfkeyle yanarlar.");
        AddTR(15, "Boşluk Parçalayıcı", "Gerçekliğin kendisini yırtabilen pençeler. Cemiyet Aurena'nın sırada ne keşfedeceğinden korkar.");
        AddTR(16, "Göksel Pençeler", "Saf yıldız ışığıyla emdirilmiş pençeler. Her darbe, tanrıların gizlemek istediği bilgi için bir duadır.");
        AddTR(17, "Büyük Bilge Pençeleri", "Cemiyetin büyük bilgesinin törensel pençeleri. Aurena bunları onların ikiyüzlülüğünün kanıtı olarak kullanır.");
        AddTR(18, "Yaşam Dokuyucusu Pençeleri", "Aurena'nın başyapıtı - yaşam gücünü yok etmeden çalabilen pençeler. Cemiyet buna sapkınlık der.");
        
        // Bismuth Silahları (19-36)
        AddTR(19, "Boş Büyü Kitabı", "Doldurulmayı bekleyen boş bir büyü kitabı. Bismuth'un kendisi gibi, sayfaları sonsuz potansiyel içerir.");
        AddTR(20, "Çırak Kitabı", "Yeni başlayanlar için bir büyü kitabı. Bismuth olan kör kız, büyü hayaliyle sayfalarını parmağıyla izledi.");
        AddTR(21, "Kristal Kodeks", "Sayfaları ince mücevherlerden yapılmış bir kitap. Bismuth'un kristal doğasıyla rezonansa girer.");
        AddTR(22, "Gezgin Günlüğü", "Asla görmemiş ama her zaman bilmiş kör kızın gözlemlerini kaydeden bir seyahat günlüğü.");
        AddTR(23, "Mücevherlere Giriş", "Kristal büyüsü hakkında temel bir rehber. Kelimeleri Bismuth'un gökkuşağı renkli vücudu gibi parlar.");
        AddTR(24, "Körün Klasiği", "Körler için kabartmalı harflerle yazılmış bir kitap. Bismuth onu dokunuşla ve hafızayla okur.");
        AddTR(25, "Kor Sözler Kitabı", "Sayfaları sonsuza dek yanan bir kitap. Alevler Bismuth'la hissettiği ama görmediği renklerle konuşur.");
        AddTR(26, "Buz Sözlüğü", "Ebedi buzla sarılmış bir kitap. Buz sayfaları rüyalar başlamadan önceki bilgiyi korur.");
        AddTR(27, "Işıltılı El Yazması", "İç ışık yayan bir kitap. Bismuth olmadan önce, kör kızı karanlıkta yönlendirdi.");
        AddTR(28, "Gölge Büyü Kitabı", "Işığı emen bir kitap. Karanlık sayfaları Bismuth'un bile okumaktan korktuğu sırları içerir.");
        AddTR(29, "Canlı Büyü Kitabı", "Kör kızla birleşmeyi seçen bilinçli bir kitap. Birlikte daha büyük bir şey oldular.");
        AddTR(30, "Prizmatik Kodeks", "Büyüyü bileşen renklerine ayıran bir kitap. Bismuth dünyayı kırdığı ışık aracılığıyla görür.");
        AddTR(31, "Boşluk Yıllıkları", "Karanlıkta kaybolanların anılarını kaydeden bir kitap. Bismuth onları onurlandırmak için hikayelerini okur.");
        AddTR(32, "Mücevher Kalbi Büyü Kitabı", "Kör kızla birleşen orijinal büyü kitabı. Sayfaları kristal yaşamla atar.");
        AddTR(33, "Kör Edici Işık Kitabı", "Görenleri kör edebilecek kadar parlak bir kitap. Bismuth için sadece sıcaklıktır.");
        AddTR(34, "Ayaza Bağlı Yıllıklar", "Zamanda donmuş eski bir kitap. Kehanetleri bir kız gibi yürüyen bir mücevherden bahseder.");
        AddTR(35, "İsimsiz Kitap", "Görme duyusu olmayan kız gibi, adsız bir kitap. Birlikte kimliklerini buldular.");
        AddTR(36, "Cehennem Klasiği", "Körlüğün kendisini tanımlamasına izin vermeyi reddeden bir hayalperestin tutkusuyla yanan bir kitap.");
        
        // Lacerta Silahları (37-54)
        AddTR(37, "Acemi Tüfeği", "Kraliyet Muhafızlarının acemilerine verilen standart silah. Lacerta ilk eğitim haftası bitmeden önce ustalaştı.");
        AddTR(38, "Avcı Uzun Namlusu", "Güvenilir bir av tüfeği. Lacerta Muhafızlara katılmadan önce ailesini geçindirmek için benzerini kullandı.");
        AddTR(39, "Devriye Karabinası", "Kompakt ve güvenilir. Krallığın sınırlarındaki uzun devriyeler için mükemmel.");
        AddTR(40, "Keskin Nişancı Tüfeği", "Gücün üzerinde hassasiyeti değer verenlere göre tasarlanmış dengeli bir tüfek.");
        AddTR(41, "Kara Barut Tüfeği", "Eski ama güvenilir. Kara barutun kokusu Lacerta'ya ilk avını hatırlatır.");
        AddTR(42, "İzci Tüfeği", "İzci görevleri için hafif tüfek. Hız ve gizlilik ham gücü aşar.");
        AddTR(43, "Kraliyet Muhafızları Tüfeği", "Lacerta'nın görev sırasında taşıdığı silah. Her çizik bir hikaye anlatır.");
        AddTR(44, "Keskin Nişancının Gururu", "Asla ıskalamayanlara mahsus hassas tüfek. Lacerta'nın kızıl gözü başkalarının göremediğini görür.");
        AddTR(45, "Alev Uzun Namlu", "Ateşli mermilerle yüklü. Düşmanlar mermi isabet ettikten çok sonra yanar.");
        AddTR(46, "Ayaz Isırığı Karabina", "Mutlak sıfıra kadar soğutulmuş mermiler atar. Hedefler ölümcül darbeden önce salyangoz hızına yavaşlar.");
        AddTR(47, "Gece Tüfeği", "Karanlıkta avlanmak için bir silah. Atışları gölge kadar sessizdir.");
        AddTR(48, "Şafak Tüfeği", "Şafak ışığıyla kutsanmış. Atışları karanlığı ve aldatmayı deler.");
        AddTR(49, "Kızıl Göz", "Lacerta'nın efsanevi bakışından adını almış. Bu tüfeğin görüş alanından kaçan hedef yoktur.");
        AddTR(50, "Kraliyet İnfazcısı", "Muhafızların seçkinlerine ayrılmış. Lacerta bu tüfeği krallığı kurtardıktan sonra aldı.");
        AddTR(51, "Güneş Topu", "Yoğunlaşmış ışık atan tüfekler. Her atış minyatür bir gün doğumu.");
        AddTR(52, "Boşluk Avcısı", "Saf karanlık mermiler atan tüfekler. Hedefler iz bırakmadan kaybolur.");
        AddTR(53, "Mutlak Sıfır", "Varlığın en soğuk noktası. Zaman bile önünde donar.");
        AddTR(54, "Ejderha Nefesi", "Patlayıcı ateşli mermilerle yüklü tüfekler. Krallığın düşmanları kükremesinden korkmayı öğrendi.");
        
        // Mist Silahları (55-70)
        AddTR(55, "Eskrim Kılıcı", "Mist'in gençliğinden kalma eğitim kılıcı. O zaman bile Astrid'deki her eğitmeni geçti.");
        AddTR(56, "Asil Efsun", "Mist'in asil soyuna uygun bir efsun. Hafif, zarif, ölümcül.");
        AddTR(57, "Düellocunun Efsunu", "Bire bir savaş için tasarlanmış efsun. Mist asla bir düello kaybetmedi.");
        AddTR(58, "Hızlı Efsun", "Mist'in kolunun uzantısı kadar hafif.");
        AddTR(59, "Astrid Efsunu", "Astrid'in en iyi demirhanelerinde dövülmüş. Kraliyet zanaatkarlığının sembolü.");
        AddTR(60, "Dansçının Efsunu", "Savaşı sanat olarak görenlere mahsus efsun. Mist'in hareketleri şiirseldir.");
        AddTR(61, "Alevli Efsun", "Alevlerle çevrili efsun. Mist'in öfkesi kararlılığı kadar sıcaktır.");
        AddTR(62, "Gölge Efsunu", "Karanlıktan vuran efsun. Düşmanlar Mist'in hareket ettiğini görmeden düşerler.");
        AddTR(63, "Ayaz Isırığı Efsun", "Donmuş çelikten efsun. Dokunuşu bedeni ve ruhu uyuşturur.");
        AddTR(64, "Camilla'nın Lütfu", "Mist'in değer verdiği birinden bir hediye. Anılarını onurlandırmak için savaşır.");
        AddTR(65, "Siper Efsunu", "Savunma ve saldırı için tasarlanmış efsun. Mist her saldırıyı fırsata çevirir.");
        AddTR(66, "Işıltılı Efsun", "İç ışık yayan efsun. Yalanları ve gölgeleri deler.");
        AddTR(67, "Cehennem Efsunu", "Sönmeyen şevkle yanan efsun. Mist'in kararlılığı söndürülemez.");
        AddTR(68, "Gece Yarısı Efsunu", "Mutlak karanlıkta dövülmüş efsun. Ölümün sessizliğiyle vurur.");
        AddTR(69, "Kış Efsunu", "Ebedi buzdan oyulmuş efsun. Soğukluğu sadece Mist'in savaştaki konsantrasyonuyla eşleşebilir.");
        AddTR(70, "Eğilmez Efsun", "Astrid'in en büyük düellocusunun efsanevi efsunu. Mist bu unvanı sayısız zaferle kazandı.");
        
        // Nachia Silahları (71-88)
        AddTR(71, "Fidan Asası", "Ruh ormanından genç bir dal. Fidanlar bile Nachia'nın çağrısına cevap verir.");
        AddTR(72, "Ruh Çağırıcı", "Orman ruhlarının seslerini yönlendirmek için oyulmuş asa. Nachia'ya sırları fısıldarlar.");
        AddTR(73, "Orman Dalı", "Hala büyüyen canlı bir dal. Ormanın büyüsü içinden akar.");
        AddTR(74, "Vahşi Asa", "Nachia'nın kendisi gibi vahşi ve öngörülemez. Ruhlar kaotik enerjisini sever.");
        AddTR(75, "Koruyucu Asası", "Ormanı koruyanlar tarafından taşınır. Nachia bu sorumlulukla doğdu.");
        AddTR(76, "Ruh Dünyası Değneği", "Maddi dünya ile ruh dünyasını birleştiren değnek. Nachia ikisi arasında yürür.");
        AddTR(77, "Ayaz Ağacı Asası", "Ebedi kışta donmuş asa. Kuzeyin soğuk ruhları çağrısına cevap verir.");
        AddTR(78, "Gölge Kökü Asası", "Ormanın en derin kısımlarında büyür. Gölge ruhları etrafında dans eder.");
        AddTR(79, "Kor Ağacı Asası", "Asla yanmayı bırakmayan asa. Ateş ruhları sıcaklığına çekilir.");
        AddTR(80, "Güneş Kutsaması Asası", "Şafak ruhlarıyla kutsamış. Işığı kayıp ruhları eve yönlendirir.");
        AddTR(81, "Fenrir'in Dişleri", "Tepesinde ruh kurdu dişleri olan asa. Nachia'nın sadık yoldaşı gücünü yönlendirir.");
        AddTR(82, "Eski Meşe Asası", "Bin yıllık meşeden oyulmuş. En eski ruhlar dikildiğini hatırlar.");
        AddTR(83, "Dünya Ağacı Dalı", "Efsanevi Dünya Ağacından bir dal. Tüm orman ruhları önünde eğilir.");
        AddTR(84, "Ruh Kralının Asası", "Ruh Kralının kendi asası. Nachia ormanı koruyarak kazandı.");
        AddTR(85, "Ebedi Ayaz Asası", "Ormanın donmuş kalbinden ebedi buz asası. Kış ruhları emirlerine itaat eder.");
        AddTR(86, "Anka Kuşu Tünemi", "Ateş ruhlarının yuva yaptığı asa. Alevleri yıkım değil yeniden doğuş getirir.");
        AddTR(87, "Işıltılı Koru Asası", "Bin ateş böceğinin ışığını yayan asa. Umut ruhları içinde dans eder.");
        AddTR(88, "Boşluk Koruyucusu Asası", "Dünyanın sınırlarını koruyan asa. Gölge ruhları taşıyıcısını korur.");
        
        // Shell Silahları (89-106)
        AddTR(89, "Acemi Katanası", "Kılıç yolunu öğrenenler için temel samuray kılıcı. Shell birkaç gün içinde ustalaştı.");
        AddTR(90, "Sessizlik Katanası", "Ses çıkarmamak için tasarlanmış samuray kılıcı. Shell'in kurbanları ölümün geldiğini asla duymaz.");
        AddTR(91, "Suikastçi Katanası", "İnce ve hassas. Shell'in sadece doğru yere bir darbeye ihtiyacı var.");
        AddTR(92, "Öldürme Katanası", "Verimli. Pratik. Acımasız. Shell'in kendisi gibi.");
        AddTR(93, "Avcı Katanası", "Avlanmak için samuray kılıcı. Shell hedef ortadan kaldırılana kadar durmaz.");
        AddTR(94, "Boşluk Katanası", "Shell'in hissettiği boşluktan adını almış. Ya da hissetmediği.");
        AddTR(95, "İnfazcının Katanası", "Sayısız hayatı sonlandıran samuray kılıcı. Shell hiçbiri için bir şey hissetmez.");
        AddTR(96, "Hassasiyet Katanası", "Zayıflıkları kullanmak için ayarlanmış samuray kılıcı. Shell en uygun saldırıyı hesaplar.");
        AddTR(97, "Ayaz Isırığı Katana", "Soğuk kötülüğün samuray kılıcı. Soğukluğu Shell'in boş kalbiyle eşleşir.");
        AddTR(98, "Arınma Katanası", "Karanlık iş için parlak samuray kılıcı. Shell ironiyi hissetmez.");
        AddTR(99, "Gölge Katanası", "Gölgeleri içen samuray kılıcı. Shell görünmeden hareket eder, sessizce vurur.");
        AddTR(100, "Kızgın Hançer", "İç ateşle ısıtılmış samuray kılıcı. Shell'in hedefleri kanamadan önce yanar.");
        AddTR(101, "Mükemmel Katilin Aleti", "Nihai katilin nihai aleti. Shell bu kılıç için doğdu.");
        AddTR(102, "Acımasız Bıçak", "Asla körelmeyen samuray kılıcı, yorulmayan bir takipçi tarafından sallanır.");
        AddTR(103, "Cehennem Suikastçisi", "Cehennem ateşinde dövülmüş samuray kılıcı. Shell'in tek amacının yoğunluğuyla yanar.");
        AddTR(104, "Boşluk Katanası", "Mutlak karanlığın samuray kılıcı. Shell gibi, sadece bitirmek için var olur.");
        AddTR(105, "Donmuş Katana", "Ebedi buzdan samuray kılıcı. Hükmü Shell kadar soğuk ve kesindir.");
        AddTR(106, "Işıltılı Son", "Parlak samuray kılıcının nihai formu. Shell El adına hükümler verir.");
        
        // Vesper Silahları (107-124)
        AddTR(107, "Acemi Asası", "Güneş Alevi Tarikatının yeni üyeleri için basit asa. Vesper yolculuğuna buradan başladı.");
        AddTR(108, "Yargı Çekici", "El'in iradesini yerine getirmek için kullanılan savaş çekici. Her darbe kutsal bir hükümdür.");
        AddTR(109, "Ateş Bekçisinin Sopası", "Kutsal ateşi koruyanların taşıdığı silah. Darbeleri pisliği arındırır.");
        AddTR(110, "Rahip Kalkanı", "Tarikatın rahipleri için basit kalkan. İnanç en iyi savunmadır.");
        AddTR(111, "Engizisyon Kalkanı", "Engizisyoncuların taşıdığı kalkan. Sayısız sapkının düşüşünü gördü.");
        AddTR(112, "Alacakaranlık Savunucusu", "Işık ve karanlık arasında koruyan kalkan.");
        AddTR(113, "Fanatik Çekici", "Dini fanatizmle yanan savaş çekici. Düşmanlar önünde titrer.");
        AddTR(114, "Arındırıcı Ağır Çekiç", "Kötülüğü arındırmak için kullanılan devasa savaş çekici. Bir darbe günahı parçalar.");
        AddTR(115, "El'in Yargısı", "El tarafından bizzat kutsanmış savaş çekici. Hükmüne itiraz edilemez.");
        AddTR(116, "İnanç Kalesi", "Yok edilemez kalkan. İnanç sönmediği sürece kırılmaz.");
        AddTR(117, "Kutsal Alev Kalkanı", "Kutsal ateşle çevrili kalkan. Ona dokunan kötülük yakılır.");
        AddTR(118, "Engizisyon Kalesi", "Yüksek engizisyoncunun kalkanı. Vesper acımasız hizmetle kazandı.");
        AddTR(119, "Şafak Kırıcı", "Karanlığı parçalayan efsanevi savaş çekici. Vesper El'in öfkesinin vücut bulmuş halini sallar.");
        AddTR(120, "Kutsal Yıkıcı", "Başrahip tarafından kutsanmış savaş çekici. Darbeleri kutsal öfkenin ağırlığını taşır.");
        AddTR(121, "El'in Sağ Eli", "Tarikatın en kutsal silahı. Vesper El'in iradesinin seçilmiş aletidir.");
        AddTR(122, "Alev Kalesi", "Tarikatın nihai savunması. El'in ışığı Vesper'i her zarardan korur.");
        AddTR(123, "Ebedi Teyakkuz", "Sarsılmaz kalkan. Vesper gibi, her zaman karanlığı gözetler.");
        AddTR(124, "Alacakaranlık Şövalyesinin Yemini", "Tarikat liderinin kalkanı. Vesper ruhunu El'e bağlamaya yemin etti.");
        
        // Yubar Silahları (125-142)
        AddTR(125, "Yeni Doğmuş Yıldız", "Yeni oluşan bir yıldız. Yubar tüm yıldızlar bu kadar küçükken hatırlar.");
        AddTR(126, "Kozmik Kor", "Yıldız ateşinin parçası. Yaratılışın kendisinin sıcaklığıyla yanar.");
        AddTR(127, "Yıldız Tozu Küresi", "Şekillendirilmeyi bekleyen sıkıştırılmış yıldız tozu. Yubar bu maddeden galaksiler dokur.");
        AddTR(128, "Rüya Katalizörü", "Rüya enerjisini artıran küre. Yubar'ın ilk deneylerinden.");
        AddTR(129, "Nebula Çekirdeği", "Uzak bir nebulanın kalbi. Yubar kozmik gobleninden kopardı.");
        AddTR(130, "Göksel Tohum", "Yıldız olacak tohum. Yubar sayısız böyle tohum yetiştirir.");
        AddTR(131, "Süpernova Parçası", "Patlayan bir yıldızın parçası. Yubar ölümüne tanık oldu ve gücünü korudu.");
        AddTR(132, "Yerçekimi Kuyusu", "Yerçekimini sıkıştıran küre. Uzayın kendisi etrafında bükülür.");
        AddTR(133, "Boşluk Tekilliği", "Sonsuz yoğunluk noktası. Yubar dünyalar yaratmak ve yok etmek için kullanır.");
        AddTR(134, "Güneş Tacı", "Güneşin dış atmosferinin parçası. Yıldızın öfkesiyle yanar.");
        AddTR(135, "Donmuş Kuyruklu Yıldız", "Yakalanmış kuyruklu yıldız çekirdeği. Kozmosun derinliklerinden sırlar taşır.");
        AddTR(136, "Yıldız Ocağı", "Bir yıldız çekirdeğinin minyatürü. Yubar yeni gerçeklik dövmek için kullanır.");
        AddTR(137, "Büyük Patlama Kalıntısı", "Yaratılışın Büyük Patlamasının parçası. Evrenin kökenini içerir.");
        AddTR(138, "Kozmik Dokuma Tezgahı", "Gerçekliği dokuyan alet. Yubar kaderi yeniden şekillendirmek için kullanır.");
        AddTR(139, "Işıltılı Yaratılış", "Yaratılışın kendisinin ışığı. Yubar ilk yıldızları doğurmak için kullandı.");
        AddTR(140, "Entropi Kalbi", "Kozmik çürümenin özü. Her şey sona erer ve Yubar ne zaman olduğuna karar verir.");
        AddTR(141, "Mutlak Sıfır", "Varlığın en soğuk noktası. Zaman bile önünde donar.");
        AddTR(142, "Felaket Çekirdeği", "Kozmik yıkımın kalbi. Yubar sadece kritik anlarda serbest bırakır.");
        
        // Yüzükler (143-162)
        AddTR(143, "Hayalcinin Yüzüğü", "Rüya dünyasına yeni girenlerin taktığı basit yüzük. Zayıf umut yayar.");
        AddTR(144, "Yıldız Tozu Yüzüğü", "Yoğunlaşmış yıldız tozundan dövülmüş. Hayalciler tehlikede daha parlak parladığını söyler.");
        AddTR(145, "Uyku İşareti", "Huzurlu uyku işareti oyulmuş. Taşıyanlar yaralardan daha hızlı iyileşir.");
        AddTR(146, "Ruh Yüzüğü", "Gezici ruhları yakalayan yüzük. Işıkları karanlık rüyalar boyunca yolları aydınlatır.");
        AddTR(147, "Gezginin Yüzüğü", "Rüya alemlerinin gezginleri tarafından takılır. Sayısız unutulmuş yol gördü.");
        AddTR(148, "Kabus Parçası Yüzüğü", "Yenilmiş kabusların parçalarını içeren yüzük. Karanlığı cesurlara güç verir.");
        AddTR(149, "Anı Koruyucusu", "Değerli anıları koruyan yüzük. Smoothie dükkânında benzerlerini satıyor.");
        AddTR(150, "Ay Arcanum Yüzüğü", "Ay Arcanum Cemiyetinden bir yüzük. Aurena sürgünden önce benzerini taktı.");
        AddTR(151, "Ruh Ormanı İşareti", "Nachia'nın ormanının ruhlarıyla kutsamış yüzük. Doğa gücü içinden akar.");
        AddTR(152, "Kraliyet Muhafızları Rozeti", "Lacerta'nın görev sırasında taktığı yüzük. Hala sorumluluk ağırlığını taşır.");
        AddTR(153, "Düellocunun Şanı", "Asil düellocular arasında aktarılan yüzük. Mist'in ailesi böyle yüzüklere değer verir.");
        AddTR(154, "Alev Koru Muskası", "Kutsal ateşin mühürlenmiş kıvılcımı. Vesper'in tarikatı bu kalıntıları korur.");
        AddTR(155, "Büyü Kitabı Parçası", "Bismuth'un kristal büyüsünden yapılmış yüzük. Arcanum enerjisiyle vızıldar.");
        AddTR(156, "Avcı İşareti", "Avı izleyen yüzük. Shell'in kontrolörleri aletlerini izlemek için kullanır.");
        AddTR(157, "El'in Kutsaması", "El'in kendisi tarafından kutsamış yüzük. Işığı kabusları dağıtır ve yaralıları iyileştirir.");
        AddTR(158, "Kabus Lordu İşareti", "Yenilmiş bir kabus lordundan alınmış. Karanlık gücü bozar ve güçlendirir.");
        AddTR(159, "Astrid Mirası", "Astrid'in asil evinden bir yüzük. Mist'in ataları sayısız düelloda taktı.");
        AddTR(160, "Kâhinin Gözü", "Rüyaların örtüsünden görebilen yüzük. Geçmiş, şimdi ve gelecek karışır.");
        AddTR(161, "İlkel Rüya Yüzüğü", "Rüyaların şafağında dövülmüş. İlk hayalcinin özünü içerir.");
        AddTR(162, "Ebedi Uyku", "En derin uykuya dokunan yüzük. Taşıyanlar ne ölümden ne de uyanmaktan korkar.");
        
        // Muskalar (163-183)
        AddTR(163, "Hayalcinin Kolyesi", "Rüya dünyasına yeni girenlerin taktığı basit kolye. Zayıf umut yayar.");
        AddTR(164, "Ateş Dokunuşu Kolyesi", "Ateşle dokunulmuş kolye. Sıcaklığı soğuk ve korkuyu dağıtır.");
        AddTR(165, "Kızıl Gözyaşı", "Kan damlası şeklinde yakut. Gücü fedakarlıktan gelir.");
        AddTR(166, "Kehribar Muska", "Eski bir yaratık içeren kehribar. Anıları taşıyanı güçlendirir.");
        AddTR(167, "Alacakaranlık Madalyası", "Işık ve karanlık arasında yanıp sönen madalya. Vesper'in yeni üyeleri bunları takar.");
        AddTR(168, "Yıldız Tozu Kolyesi", "Kristalize yıldızlarla serpiştirilmiş kolye. Smoothie bunları düşen rüyalardan toplar.");
        AddTR(169, "Zümrüt Göz", "Yanılsamaları görebilen yeşil mücevher. Kabuslar bakışından kaçamaz.");
        AddTR(170, "Orman Ruhu Kalbi", "Orman ruhlarının kutsamasını içeren mücevher. Nachia'nın koruyucuları bunlara değer verir.");
        AddTR(171, "Ay Arcanum İşareti", "Yasak arşivlerden işaret. Aurena bunları incelemek için her şeyi riske atar.");
        AddTR(172, "Bismuth Kristali", "Kristal saf büyünün parçası. Arcanum enerjisiyle rezonansa girer.");
        AddTR(173, "Alev Koru Muskası", "Kutsal ateşin mühürlenmiş kıvılcımı. Vesper'in tarikatı bu kalıntıları korur.");
        AddTR(174, "Kabus Dişi Kolyesi", "Güçlü bir kabustan bir diş, şimdi bir ganimet. Shell hatırlatma olarak bir tane takar.");
        AddTR(175, "Kraliyet Muhafızları Madalyası", "Kraliyet Muhafızlarından onur madalyası. Lacerta sürgünden önce birçok kazandı.");
        AddTR(176, "Astrid Arması", "Astrid ailesinin asil arması. Mist'in aile mirası bu zincirde asılı.");
        AddTR(177, "El'in Kutsal Gözyaşı", "El'in kendisi tarafından dökülen gözyaşı. Işığı en karanlık kabuslardan korur.");
        AddTR(178, "İlkel Zümrüt", "İlk ormandan mücevher. Eski ruhların rüyalarını içerir.");
        AddTR(179, "Kabus Lordunun Gözü", "Yenilmiş kabus lordunun gözü. Tüm korkuları görür ve kullanır.");
        AddTR(180, "Alevin Kalbi", "Kutsal ateşin kendisinin çekirdeği. Sadece en dindarlar taşıyabilir.");
        AddTR(181, "Kâhinin Kehaneti", "Tüm olası gelecekleri görebilen muska. Gerçeklik kehanetleri önünde bükülür.");
        AddTR(182, "Boşluk Gezgininin Pusulası", "Boşluğu gösteren muska. Onu takip edenler asla aynı şekilde dönmez.");
        AddTR(183, "Midas Muskası", "Altın rüyasıyla dokunulmuş efsanevi muska. Yenilen düşmanlar ekstra altın düşürür.");
        AddTR(478, "Yıldız Tozu Toplayıcısı", "Yenilen düşmanlardan kristalleşmiş yıldız tozu toplayan efsanevi muska. Her öldürme değerli rüya özü üretir (temel 1-5, yükseltme seviyesi ve canavar gücüne göre ölçeklenir).");
        
        // Kemerler (184-203)
        AddTR(184, "Gezginin Kemeri", "Rüyalar arasında dolaşanların taktığı basit kumaş kemer. Ruhu demirleme yapar.");
        AddTR(185, "Hayalcinin İpi", "Rüya iplerinden dokunmuş ip. Her kalp atışıyla hafifçe atar.");
        AddTR(186, "Yıldız Tozu Tokası", "Kristalize yıldız tozuyla süslenmiş kemer. Smoothie benzer aksesuarlar satıyor.");
        AddTR(187, "Nöbetçi Kemeri", "Rüya bekçilerinin taktığı sağlam kemer. Asla gevşemez, asla başarısız olmaz.");
        AddTR(188, "Gizemli Kemer", "Zayıf büyüyle emdirilmiş kemer. Kabuslar yaklaştığında batar.");
        AddTR(189, "Avcı Kemeri", "Malzeme çantalı deri kemer. Rüyalarda uzun yolculuklar için gerekli.");
        AddTR(190, "Hacı Kemeri", "El'in ışığını arayanlar tarafından takılır. En karanlık rüyalarda teselli sunar.");
        AddTR(191, "Kabus Avcısı Kemeri", "Yenilmiş kabuslardan yapılmış kemer. Özleri taşıyanı güçlendirir.");
        AddTR(192, "Arcanum Kemeri", "Ay Arcanum Cemiyetinden kemer. Büyü enerjisini artırır.");
        AddTR(193, "Alev Kemeri", "Vesper'in tarikatı tarafından kutsamış kemer. Koruma ateşiyle yanar.");
        AddTR(194, "Düellocunun Kılıç Kemeri", "Astrid'in asil savaşçılarının taktığı zarif kemer. Kılıçları ve onuru taşır.");
        AddTR(195, "Ruh Dokuyucunun İpi", "Orman ruhlarının Nachia için dokuduğu kemer. Doğa enerjisiyle vızıldar.");
        AddTR(196, "Suikastçi Alet Kemeri", "Gizli bölmeleri olan kemer. Shell'in kontrolörleri aletlerini donatmak için kullanır.");
        AddTR(197, "Silahşor Kılıfı", "Lacerta'nın ateşli silahları için tasarlanmış kemer. Hızlı çekiş, daha hızlı öldürme.");
        AddTR(198, "El'in Kutsal Kemeri", "El'in kendisi tarafından kutsamış kemer. Işığı ruhu tüm karanlığa karşı demirler.");
        AddTR(199, "Kabus Lordunun Zinciri", "Kabus lordunun zincirlerinden dövülmüş kemer. Korkuyu taşıyanın iradesine bağlar.");
        AddTR(200, "İlkel Rüya Kemeri", "İlk rüyadan kemer. Yaratılışın kendisinin yankısını içerir.");
        AddTR(201, "Dünya Ağacı Kökü Kemeri", "Dünya Ağacının köklerinden büyüyen kemer. Nachia'nın ormanı yaratılışını kutsadı.");
        AddTR(202, "Astrid Yadigârı", "Astrid ailesinin aile kemeri. Mist'in kanı dokusuna dokunmuş.");
        AddTR(203, "Alacakaranlık Engizisyoncu Kemeri", "Vesper'in en üst seviye kemeri. Önünde duran herkesi yargılar.");
        
        // Zırh Setleri - Kemik Nöbetçi (204-215)
        AddTR(204, "Kemik Nöbetçi Maskesi", "Düşmüş bir kabusun kafatasından oyulmuş maske. Boş gözleri yanılsamaları görür.");
        AddTR(205, "Ruh Dünyası Koruyucusu Miğferi", "Nachia'nın orman ruhlarıyla emdirilmiş miğfer. Koruma fısıltıları taşıyanı sarar.");
        AddTR(206, "Kabus Lordu Tacı", "Fethedilmiş kabuslardan dövülmüş taç. Taşıyan korkunun kendisine hükmeder.");
        AddTR(207, "Kemik Nöbetçi Yeleği", "Rüyada ölen yaratıkların kaburgalarından zırh. Korumaları ölümün ötesinde sürer.");
        AddTR(208, "Ruh Dünyası Koruyucusu Göğüslüğü", "Kristalleşmiş rüyalardan dokunmuş göğüslük. Gelen darbelere göre hareket eder ve adapte olur.");
        AddTR(209, "Kabus Lordu Göğüslüğü", "Karanlık enerjiyle atan zırh. Kabuslar taşıyanın önünde eğilir.");
        AddTR(210, "Kemik Nöbetçi Bacaklıkları", "Kabus kemikleriyle güçlendirilmiş bacak zırhı. Asla yorulmazlar, asla pes etmezler.");
        AddTR(211, "Ruh Dünyası Koruyucusu Bacaklıkları", "Orman ruhlarıyla kutsamış bacak zırhı. Taşıyan yapraklar arasında rüzgar gibi hareket eder.");
        AddTR(212, "Kabus Lordu Bacaklıkları", "Gölgeleri içen bacak zırhı. Karanlık her adıma güç verir.");
        AddTR(213, "Kemik Nöbetçi Çizmeleri", "Rüya alemlerinde sessizce yürümek için çizmeler. Ölüler ses çıkarmaz.");
        AddTR(214, "Ruh Dünyası Koruyucusu Demir Çizmeleri", "Rüya alemlerinde iz bırakmayan çizmeler. Kabus avcıları için mükemmel.");
        AddTR(215, "Kabus Lordu Çizmeleri", "Dünyalar arası geçiş yapan çizmeler. Gerçeklik her adımın altında bükülür.");
        
        // Aurena Zırhı (216-243)
        AddTR(216, "Aurena'nın Çırak Şapkası", "Ay Arcanum acemileri tarafından takılan basit şapka.");
        AddTR(217, "Aurena'nın Gizemli Sarığı", "Zayıf ay ışığı enerjisiyle emdirilmiş sarık.");
        AddTR(218, "Aurena'nın Büyüleyici Tacı", "Taşıyanın büyü yakınlığını artıran taç.");
        AddTR(219, "Aurena'nın Eski Tacı", "Nesillerden aktarılan eski bir kalıntı.");
        AddTR(220, "Aurena'nın Şahin Bandı", "Gökyüzü ruhlarıyla kutsamış saç bandı.");
        AddTR(221, "Aurena'nın Güç Tacı", "Arcanum gücü yayan güçlü taç.");
        AddTR(222, "Aurena'nın Ay Işığı Tacı", "Ay Arcanum ustasının efsanevi tacı.");
        AddTR(223, "Aurena'nın Mavi Cübbesi", "Ay büyücülerinin giydiği basit cübbe.");
        AddTR(224, "Aurena'nın Büyücü Cübbesi", "Pratik yapan büyücülerin sevdiği cübbe.");
        AddTR(225, "Aurena'nın Büyü Dokunmuş Cübbesi", "Büyülü ipliklerle dokunmuş cübbe.");
        AddTR(226, "Aurena'nın Aydınlatıcı Cübbesi", "Ay ışığı bilgeliğini yönlendiren cübbe.");
        AddTR(227, "Aurena'nın Rüya Örtüsü", "Kristalleşmiş rüyalardan dokunmuş örtü.");
        AddTR(228, "Aurena'nın Kraliyet Pullu Cübbesi", "Ay kraliyet ailesine uygun cübbe.");
        AddTR(229, "Aurena'nın Buz Kraliçesi Cübbesi", "Buz Kraliçesinin kendisinin efsanevi cübbesi.");
        AddTR(230, "Aurena'nın Mavi Bacaklıkları", "Hevesli büyücülerin giydiği basit bacaklık.");
        AddTR(231, "Aurena'nın Elyaf Eteği", "Serbest hareket sağlayan hafif etek.");
        AddTR(232, "Aurena'nın Elf Bacaklıkları", "Elfler tarafından tasarlanmış zarif bacaklık.");
        AddTR(233, "Aurena'nın Aydınlatıcı Bacaklıkları", "Bilgelikle kutsamış bacaklık.");
        AddTR(234, "Aurena'nın Buzul Eteği", "Ayaz büyüsüyle emdirilmiş etek.");
        AddTR(235, "Aurena'nın Ayaz Şalvarı", "Soğukluk gücü yayan şalvar.");
        AddTR(236, "Aurena'nın Muhteşem Bacaklıkları", "Eşsiz zarafetin efsanevi bacaklıkları.");
        AddTR(237, "Yumuşak Ay Işığı Terlikleri", "Ay Arcanum rahiplerinin giydiği yumuşak ayakkabılar. Her adımı ay ışığıyla yastıklar.");
        AddTR(238, "Hacı Sandaletleri", "Cemiyetin büyüklerince kutsamış basit sandaletler. Aurena hala sıcaklıklarını hatırlar.");
        AddTR(239, "Doğu Gizemi Ayakkabıları", "Uzak diyarlardan egzotik ayakkabılar. Şifa enerjisini toprak yoluyla yönlendirirler.");
        AddTR(240, "Rüya Yürüyücüsünün Lütfu", "Uyanık dünya ile rüya dünyası arasında yürüyen çizmeler. Kayıp ruhları arayan şifacılar için mükemmel.");
        AddTR(241, "Ruh Yürüyücüsü Çizmeleri", "Ölmüş şifacıların özüyle emdirilmiş çizmeler. Bilgelikleri her adımı yönlendirir.");
        AddTR(242, "Ruh İzleyicisi Çizmeleri", "Yaralıları izleyen karanlık çizmeler. Aurena yardıma ihtiyacı olanları bulmak için kullanır... ya da kendisine zarar verenleri.");
        AddTR(243, "Düşmüş Bilge Kanatları", "Saf kalplere uçma yeteneği verdiği söylenen efsanevi çizmeler. Aurena'nın sürgünü niteliğini kanıtladı.");
        
        // Bismuth Zırhı (244-271)
        AddTR(244, "Bismuth'un Koni Şapkası", "Kristal çıraklar için basit şapka.");
        AddTR(245, "Bismuth'un Zümrüt Şapkası", "Zümrüt kristallerle işlenmiş şapka.");
        AddTR(246, "Bismuth'un Buzul Maskesi", "Donmuş kristallerden yapılmış maske.");
        AddTR(247, "Bismuth'un Alarharim Maskesi", "Kristal gücün eski maskesi.");
        AddTR(248, "Bismuth'un Prizmatik Miğferi", "Işığı gökkuşağına kıran miğfer.");
        AddTR(249, "Bismuth'un Yansıtıcı Tacı", "Büyü enerjisini yansıtan taç.");
        AddTR(250, "Bismuth'un Beyaz Kızgınlık Tacı", "Kristal ateşiyle yanan efsanevi taç.");
        AddTR(251, "Bismuth'un Buz Cübbesi", "Kristal büyüsüyle soğutulmuş cübbe.");
        AddTR(252, "Bismuth'un Keşiş Cübbesi", "Meditasyon için basit cübbe.");
        AddTR(253, "Bismuth'un Buzul Cübbesi", "Ebedi ayazda donmuş cübbe.");
        AddTR(254, "Bismuth'un Kristal Zırhı", "Canlı kristallerden oluşturulmuş zırh.");
        AddTR(255, "Bismuth'un Prizmatik Zırhı", "Işığın kendisini büken zırh.");
        AddTR(256, "Bismuth'un Donmuş Plaka Zırhı", "Ebedi ayazın plaka zırhı.");
        AddTR(257, "Bismuth'un Kutsal Plaka Zırhı", "Kristal tanrıları tarafından kutsamış efsanevi zırh.");
        AddTR(258, "Bismuth'un Zincirli Bacaklıkları", "Kristal halkalarla temel bacaklık.");
        AddTR(259, "Bismuth'un Elyaf Bacaklıkları", "Kristal büyücüler için hafif bacaklık.");
        AddTR(260, "Bismuth'un Zümrüt Bacaklıkları", "Zümrüt kristallerle süslenmiş bacaklık.");
        AddTR(261, "Bismuth'un Garip Pantolonu", "Garip enerjiyle atan pantolon.");
        AddTR(262, "Bismuth'un Prizmatik Bacaklıkları", "Prizmatik ışıkla parlayan bacaklık.");
        AddTR(263, "Bismuth'un Alarharim Bacaklıkları", "Alarharim'in eski bacaklıkları.");
        AddTR(264, "Bismuth'un Şahin Bacaklıkları", "Şahin Tarikatının efsanevi bacaklıkları.");
        AddTR(265, "Bilgin Terlikleri", "Kütüphanelerde uzun çalışmalar için rahat ayakkabılar. Bilgi tabanlarından akar.");
        AddTR(266, "Alarharim Sargıları", "Kayıp bir uygarlıktan eski sargılar. Unutulmuş büyüleri fısıldarlar.");
        AddTR(267, "Buz Ayakkabıları", "Ebedi buzdan oyulmuş çizmeler. Bismuth yürüdüğü yerde ayaz desenleri bırakır.");
        AddTR(268, "Ayaz Çiçeği Adımı", "Buz kristalleriyle çiçek açan zarif çizmeler. Her adım bir ayaz bahçesi yaratır.");
        AddTR(269, "Kristal Rezonans Çizmeleri", "Büyü enerjisini artıran çizmeler. Kristaller kullanılmamış güçle vızıldar.");
        AddTR(270, "Prizmatik Arcanum Çizmeleri", "Işığı saf büyü enerjisine kıran çizmeler. Gerçeklik her adımın etrafında bükülür.");
        AddTR(271, "Boşluk Yürüyücüsü Çizmeleri", "Boşluğun kendisini aşan efsanevi çizmeler. Bismuth başkalarının hayal edemeyeceği yolları görür.");
        
        // Lacerta Zırhı (272-299)
        AddTR(272, "Kertenkele Adamın Deri Miğferi", "Ejderha savaşçıları için temel miğfer.");
        AddTR(273, "Kertenkele Adamın Zincirli Miğferi", "Sağlam zincirli miğfer.");
        AddTR(274, "Kertenkele Adamın Savaşçı Miğferi", "Deneyimli savaşçıların giydiği miğfer.");
        AddTR(275, "Kertenkele Adamın Ejderha Pullu Miğferi", "Ejderha pullarından dövülmüş miğfer.");
        AddTR(276, "Kertenkele Adamın Kraliyet Miğferi", "Ejderha kraliyet ailesine uygun miğfer.");
        AddTR(277, "Kertenkele Adamın Elit Ejderha Adam Miğferi", "Ejderha Adam Tarikatının elit miğferi.");
        AddTR(278, "Kertenkele Adamın Altın Miğferi", "Ejderha Kralının efsanevi altın miğferi.");
        AddTR(279, "Kertenkele Adamın Deri Zırhı", "Ejderha savaşçıları için temel zırh.");
        AddTR(280, "Kertenkele Adamın Pullu Zırhı", "Pullarla güçlendirilmiş zırh.");
        AddTR(281, "Kertenkele Adamın Şövalye Zırhı", "Ejderha şövalyeleri için ağır zırh.");
        AddTR(282, "Kertenkele Adamın Ejderha Pullu Zincirliği", "Ejderha pullarından dövülmüş zincirlik.");
        AddTR(283, "Kertenkele Adamın Kraliyet Ejderha Adam Zincirliği", "Ejderha Adam Tarikatının kraliyet zincirliği.");
        AddTR(284, "Kertenkele Adamın Şahin Plaka Zırhı", "Şahin Tarikatının plaka zırhı.");
        AddTR(285, "Kertenkele Adamın Altın Zırhı", "Ejderha Kralının efsanevi altın zırhı.");
        AddTR(286, "Kertenkele Adamın Deri Bacaklıkları", "Ejderha savaşçıları için temel bacaklık.");
        AddTR(287, "Kertenkele Adamın Perçinli Bacaklıkları", "Perçinlerle güçlendirilmiş bacaklık.");
        AddTR(288, "Kertenkele Adamın Şövalye Bacaklıkları", "Ejderha şövalyeleri için ağır bacaklık.");
        AddTR(289, "Kertenkele Adamın Ejderha Pullu Bacaklıkları", "Ejderha pullarından dövülmüş bacaklık.");
        AddTR(290, "Kertenkele Adamın Kraliyet Bacaklıkları", "Kraliyet ailesine uygun bacaklık.");
        AddTR(291, "Kertenkele Adamın Muhteşem Bacaklıkları", "Usta işçiliğin muhteşem bacaklıkları.");
        AddTR(292, "Kertenkele Adamın Altın Bacaklıkları", "Ejderha Kralının efsanevi altın bacaklıkları.");
        AddTR(293, "Aşınmış Deri Çizmeler", "Birçok savaş görmüş basit çizmeler. Macera kokusunu taşırlar.");
        AddTR(294, "Yamalı Savaş Çizmeleri", "Sayısız kez onarılmış çizmeler. Her yama bir hayatta kalma hikayesi anlatır.");
        AddTR(295, "Çelik Savaşçı Çizmeleri", "Savaş için dövülmüş ağır çizmeler. Lacerta'yı savaş sırasında yere demirler.");
        AddTR(296, "Bekçi Bacaklıkları", "Elit koruyucuların giydiği çizmeler. Asla geri çekilmezler, asla teslim olmazlar.");
        AddTR(297, "Ejderha Pullu Bacaklıklar", "Ejderha pullarından yapılmış çizmeler. Taşıyana ejderha dayanıklılığı verir.");
        AddTR(298, "Ejderha Adam Savaş Lordunun Çizmeleri", "Eski ejderha adam generallerinin çizmeleri. Her savaş alanında korkutucu.");
        AddTR(299, "Altın Şampiyonun Çizmeleri", "Bir çağın en büyük savaşçısının giydiği efsanevi çizmeler. Lacerta'nın kaderi bekliyor.");
        
        // Mist Zırhı (300-327)
        AddTR(300, "Mist'in Sarığı", "Kimliği gizleyen basit sarık.");
        AddTR(301, "Mist'in Hafif Sarığı", "Hafif, nefes alabilen sarık.");
        AddTR(302, "Mist'in Gece Görüşü Sarığı", "Gece görüşünü artıran sarık.");
        AddTR(303, "Mist'in Şimşek Bandı", "Elektrik enerjisiyle dolu saç bandı.");
        AddTR(304, "Mist'in Kobra Başlığı", "Yılan gibi ölümcül başlık.");
        AddTR(305, "Mist'in Cehennem İzleyici Maskesi", "Avı izleyen maske.");
        AddTR(306, "Mist'in Karanlık Fısıltısı", "Ölümü fısıldayan efsanevi maske.");
        AddTR(307, "Mist'in Deri Koşum Takımı", "Çeviklik için hafif koşum takımı.");
        AddTR(308, "Mist'in Korucu Pelerini", "Hızlı hareket için pelerin.");
        AddTR(309, "Mist'in Gece Yarısı Ceketi", "Gece saldırıları için ceket.");
        AddTR(310, "Mist'in Şimşek Cübbesi", "Şimşeklerle yüklü cübbe.");
        AddTR(311, "Mist'in Voltaj Zırhı", "Elektrik enerjisiyle atan zırh.");
        AddTR(312, "Mist'in Usta Okçu Zırhı", "Bir okçuluk ustasının zırhı.");
        AddTR(313, "Mist'in Karanlık Lord Pelerini", "Karanlık Lordun efsanevi pelerini.");
        AddTR(314, "Mist'in Korucu Bacaklıkları", "Korucuların giydiği bacaklık.");
        AddTR(315, "Mist'in Orman Hayatta Kalma Bacaklıkları", "Orman hayatta kalanının bacaklıkları.");
        AddTR(316, "Mist'in Gece Yarısı Sarongu", "Gece operasyonları için sarong.");
        AddTR(317, "Mist'in Şimşek Bacaklıkları", "Şimşeklerle yüklü bacaklık.");
        AddTR(318, "Mist'in Çekirge Bacaklıkları", "Şaşırtıcı sıçramalar için bacaklık.");
        AddTR(319, "Mist'in Yeşil İblis Bacaklıkları", "İblisçe hız bacaklıkları.");
        AddTR(320, "Mist'in Ruh Yürüyücüsü Bacaklıkları", "Dünyalar arası geçiş yapan efsanevi bacaklıklar.");
        AddTR(321, "Geçici İzci Çizmeleri", "Parçalardan monte edilmiş çizmeler. Hafif ve sessiz, keşif için mükemmel.");
        AddTR(322, "Bataklık Koşucusu Çizmeleri", "Zor arazide geçiş için su geçirmez çizmeler. Mist her kestirme yolu bilir.");
        AddTR(323, "Hızlı Yürüyücü", "Taşıyanın adımlarını hızlandıran büyülü çizmeler. Vur-kaç taktiği için mükemmel.");
        AddTR(324, "Geyik Avcısı Çizmeleri", "Avı izleyen çizmeler.");
        AddTR(325, "Şimşek Vurucu Çizmeleri", "Şimşek enerjisiyle yüklü çizmeler.");
        AddTR(326, "Ateş Yürüyücüsü Savaş Çizmeleri", "Ateşte yürüyebilen çizmeler.");
        AddTR(327, "Acı Çiğneyici", "Tüm acıyı çiğneyen efsanevi çizmeler.");
        
        // Nachia Zırhı (328-355)
        AddTR(328, "Nachia'nın Kürk Şapkası", "Orman avcıları için basit şapka.");
        AddTR(329, "Nachia'nın Tüylü Başlığı", "Tüylerle süslenmiş başlık.");
        AddTR(330, "Nachia'nın Şaman Maskesi", "Ruhsal güç maskesi.");
        AddTR(331, "Nachia'nın Toprak Başlığı", "Toprak ruhlarıyla kutsamış başlık.");
        AddTR(332, "Nachia'nın Doğa Miğferi", "Doğa gücüyle emdirilmiş miğfer.");
        AddTR(333, "Nachia'nın Ağaç Tacı", "Eski bir ağaçtan büyüyen taç.");
        AddTR(334, "Nachia'nın Yaprak Tacı", "Orman Koruyucusunun efsanevi tacı.");
        AddTR(335, "Nachia'nın Kürk Zırhı", "Orman yaratıklarından yapılmış zırh.");
        AddTR(336, "Nachia'nın Yerli Zırhı", "Orman kabilelerinin geleneksel zırhı.");
        AddTR(337, "Nachia'nın Yeşil Ağaç Paltosu", "Canlı sarmaşıklardan dokunmuş palto.");
        AddTR(338, "Nachia'nın Toprak Pelerini", "Toprak ruhlarıyla kutsamış pelerin.");
        AddTR(339, "Nachia'nın Doğa Kucaklaması", "Doğayla birleşmiş zırh.");
        AddTR(340, "Nachia'nın Bataklık Yuvası Zırhı", "Derin bataklıklardan zırh.");
        AddTR(341, "Nachia'nın Yaprak Cübbesi", "Orman Koruyucusunun efsanevi cübbesi.");
        AddTR(342, "Nachia'nın Mamut Kürk Şortu", "Mamut kürkünden yapılmış sıcak şort.");
        AddTR(343, "Nachia'nın Deri Şortu", "Geleneksel av bacaklıkları.");
        AddTR(344, "Nachia'nın Geyik Bacaklıkları", "Geyik derisinden yapılmış bacaklık.");
        AddTR(345, "Nachia'nın Toprak Bacaklıkları", "Toprak ruhlarıyla kutsamış bacaklık.");
        AddTR(346, "Nachia'nın Yaprak Bacaklıkları", "Büyülü yapraklardan dokunmuş bacaklık.");
        AddTR(347, "Nachia'nın Yaban Domuzu Adamın Peştemali", "Güçlü bir yaban domuzu adamdan peştamal.");
        AddTR(348, "Nachia'nın Kanlı Av Bacaklıkları", "Kanlı avın efsanevi bacaklıkları.");
        AddTR(349, "Kürk Astarlı Çizmeler", "Sıcak, rahat çizmeler.");
        AddTR(350, "Porsuk Derisi Çizmeleri", "Porsuk derisinden yapılmış çizmeler.");
        AddTR(351, "Kobra Vuruşu Çizmeleri", "Yılan gibi hızlı çizmeler.");
        AddTR(352, "Orman Gizleyicisi Sargıları", "Ormanda sessiz sargılar.");
        AddTR(353, "Toprak Avcısı Çizmeleri", "Avı izleyen çizmeler.");
        AddTR(354, "Ateş Çiçeği Korucu Çizmeleri", "Ateş çiçekleriyle süslenmiş çizmeler. Ateşli hız ve ölümcül hassasiyet verir.");
        AddTR(355, "Kanlı Yırtıcı Çizmeleri", "Sayısız avın kanıyla bulanmış efsanevi çizmeler. Nachia en üst yırtıcı oldu.");
        
        // Shell Zırhı (356-383)
        AddTR(356, "Shell'in Hasarlı Miğferi", "Yeraltı dünyasından yıpranmış miğfer.");
        AddTR(357, "Shell'in Kırık Maskesi", "Hala koruma sunan kırık maske.");
        AddTR(358, "Shell'in Kemik Lordunun Miğferi", "Kemik Lordunun kalıntılarından dövülmüş miğfer.");
        AddTR(359, "Shell'in İskelet Miğferi", "Kemiklerden şekillendirilmiş miğfer.");
        AddTR(360, "Shell'in Ölüm Miğferi", "Ölümün kendisinin miğferi.");
        AddTR(361, "Shell'in Nokferatu İskelet Bekçisi", "Vampir kemiklerinden miğfer.");
        AddTR(362, "Shell'in İblis Miğferi", "İblis Lordunun efsanevi miğferi.");
        AddTR(363, "Shell'in Cenaze Örtüsü", "Mezardan örtü.");
        AddTR(364, "Shell'in Eski Pelerini", "Eski zamanlardan yaşlı pelerin.");
        AddTR(365, "Shell'in Nokferatu Kemik Pelerini", "Kemiklerden dokunmuş pelerin.");
        AddTR(366, "Shell'in Ruh Pelerini", "Huzursuz ruhların pelerini.");
        AddTR(367, "Shell'in Ölüm Cübbesi", "Ölümün cübbesi.");
        AddTR(368, "Shell'in Yeraltı Dünyası Cübbesi", "Derinliklerden cübbe.");
        AddTR(369, "Shell'in İblis Zırhı", "İblis Lordunun efsanevi zırhı.");
        AddTR(370, "Shell'in Hasarlı Önlüğü", "Hasarlı ama yararlı önlük.");
        AddTR(371, "Shell'in Mutasyonlu Kemik Eteği", "Mutasyonlu kemiklerden yapılmış etek.");
        AddTR(372, "Shell'in Nokferatu Dikenli Sargıları", "Dikenli sargılar.");
        AddTR(373, "Shell'in Nokferatu Et Bekçisi", "Korunmuş etten yapılmış koruma.");
        AddTR(374, "Shell'in Kanlı Bacaklıkları", "Kanla bulanmış bacaklık.");
        AddTR(375, "Shell'in Nokferatu Kan Yürüyücüsü", "Kanda yürüyen bacaklık.");
        AddTR(376, "Shell'in İblis Bacaklıkları", "İblis Lordunun efsanevi bacaklıkları.");
        AddTR(377, "Zırhlı Bacaklıklar", "Ayakların altındaki her şeyi ezen ağır metal çizmeler. Savunma en iyi saldırıdır.");
        AddTR(378, "Denizcinin Çizmeleri", "Her fırtınaya dayanıklı sağlam çizmeler. Shell'i savaş akıntılarına demirler.");
        AddTR(379, "Derin Bacaklıkları", "Okyanusun derinliklerinde dövülmüş çizmeler. Her baskıya dayanıklıdır.");
        AddTR(380, "Kemik Kırıcı Çizmeler", "Canavar kemikleriyle güçlendirilmiş çizmeler. Her adım bir tehdittir.");
        AddTR(381, "Magma Kalesi Çizmeleri", "Volkanın ateşinde dövülmüş çizmeler. Saldırganları yakan sıcaklık yayar.");
        AddTR(382, "Kan Çiğneyici Çizmeler", "Yıkım izleri bırakan vahşi çizmeler. Düşmanlar onlarla savaşmaktan korkar.");
        AddTR(383, "Sarsılmaz Bekçi Çizmeleri", "Yok edilemez bir savunucunun efsanevi çizmeleri. Shell hareketsiz bir kale olur.");
        
        // Vesper Zırhı (384-411)
        AddTR(384, "Vesper'in Cadı Şapkası", "Rüya dokuyucuları için basit şapka.");
        AddTR(385, "Vesper'in Tuhaf Başlığı", "Sırları fısıldayan başlık.");
        AddTR(386, "Vesper'in Garip Başlığı", "Garip enerjiyle atan başlık.");
        AddTR(387, "Vesper'in Kabus Enerjisi Başlığı", "Kabus enerjisinin başlığı.");
        AddTR(388, "Vesper'in Umutsuzluk Sargıları", "Umutsuzlukla beslenen sargılar.");
        AddTR(389, "Vesper'in Karanlık Büyücü Tacı", "Karanlık büyünün tacı.");
        AddTR(390, "Vesper'in Ferunbras Şapkası", "Karanlık Ustasının efsanevi şapkası.");
        AddTR(391, "Vesper'in Kırmızı Cübbesi", "Hevesli rüya büyücüleri için cübbe.");
        AddTR(392, "Vesper'in Ruh Bağları", "Ruhları yönlendiren bağlar.");
        AddTR(393, "Vesper'in Enerji Cübbesi", "Enerjiyle çıtırdayan cübbe.");
        AddTR(394, "Vesper'in Hayalet Eteği", "Hayalet enerjisinden dokunmuş etek.");
        AddTR(395, "Vesper'in Ruh Pelerini", "Ruhları içeren pelerin.");
        AddTR(396, "Vesper'in Ruh Sargıları", "Yakalanan ruhların sargıları.");
        AddTR(397, "Vesper'in Arcanum Ejderha Cübbesi", "Arcanum Ejderhasının efsanevi cübbesi.");
        AddTR(398, "Vesper'in Ruh Bacaklıkları", "Ruh enerjisiyle emdirilmiş bacaklık.");
        AddTR(399, "Vesper'in Egzotik Bacaklıkları", "Uzak alemlerden egzotik bacaklık.");
        AddTR(400, "Vesper'in Ruh Bacak Zırhı", "Ruh gücünü yönlendiren bacaklık.");
        AddTR(401, "Vesper'in Kanlı Pantolonu", "Kan büyüsüyle boyanmış pantolon.");
        AddTR(402, "Vesper'in Magma Bacaklıkları", "Büyülü ateşte dövülmüş bacaklık.");
        AddTR(403, "Vesper'in Bilgelik Bacaklıkları", "Eski bilgeliğin bacaklıkları.");
        AddTR(404, "Vesper'in Kadimler Pantolonu", "Kadimlerden efsanevi pantolon.");
        AddTR(405, "Rahip Terlikleri", "Tapınağın yeni üyelerinin giydiği yumuşak terlikler. Her adımda dua taşırlar.");
        AddTR(406, "Tapınak Ayakkabıları", "El'in hizmetkarlarının geleneksel ayakkabıları. Vesper'i inancına demirler.");
        AddTR(407, "Garip Keşiş Çizmeleri", "Yasak metinler inceleyen keşişlerin giydiği çizmeler. Bilgi ve inanç iç içe geçer.");
        AddTR(408, "Cüce Kutsamalı Sargılar", "Cüce zanaatkarlar tarafından büyülenmiş sargılar. Teknoloji tanrısallıkla buluşur.");
        AddTR(409, "Vampir İpek Terlikleri", "Vampir ipekten dokunmuş zarif terlikler. Toprağın kendisinden yaşam çekerler.");
        AddTR(410, "İblis Öldürücü Terlikleri", "Kötülüğü yok etmek için kutsamış yeşil terlikler. Her adımda iblisleri yakarlar.");
        AddTR(411, "Kabus Kovucu Çizmeleri", "Masumları kurtarmak için kabusları aşan efsanevi çizmeler. Vesper'in nihai görevi.");
        
        // Yubar Zırhı (412-439)
        AddTR(412, "Yubar'ın Kabile Maskesi", "Kabile savaşçıları için maske.");
        AddTR(413, "Yubar'ın Viking Miğferi", "Kuzey savaşçıları için miğfer.");
        AddTR(414, "Yubar'ın Boynuzlu Miğferi", "Korkunç boynuzlu miğfer.");
        AddTR(415, "Yubar'ın Dayanıklı Ix Başlığı", "Ix kabilesi için başlık.");
        AddTR(416, "Yubar'ın Ateş Korkusu Başlığı", "Korkuyla yanan başlık.");
        AddTR(417, "Yubar'ın Dayanıklı Ix Miğferi", "Ix savaşçıları için miğfer.");
        AddTR(418, "Yubar'ın Kıyamet Yüzü", "Kıyametin efsanevi yüzü.");
        AddTR(419, "Yubar'ın Ayı Kürkü", "Güçlü bir ayıdan zırh.");
        AddTR(420, "Yubar'ın Mamut Kürk Pelerini", "Bir mamutten pelerin.");
        AddTR(421, "Yubar'ın Dayanıklı Ix Cübbesi", "Ix kabilesi için cübbe.");
        AddTR(422, "Yubar'ın Magma Paltosu", "Magmada dövülmüş palto.");
        AddTR(423, "Yubar'ın Dayanıklı Ix Göğüslüğü", "Ix savaşçıları için göğüslük.");
        AddTR(424, "Yubar'ın Lav Plaka Zırhı", "Lav plaka zırhı.");
        AddTR(425, "Yubar'ın Ateş Devi Zırhı", "Ateş Devinin efsanevi zırhı.");
        AddTR(426, "Yubar'ın Dayanıklı Ix Bel Zırhı", "Ix kabilesi için bacak zırhı.");
        AddTR(427, "Yubar'ın Mutasyonlu Deri Pantolonu", "Mutasyonlu deriden yapılmış pantolon.");
        AddTR(428, "Yubar'ın Dayanıklı Ix Bacak Eteği", "Ix savaşçıları için bacak eteği.");
        AddTR(429, "Yubar'ın Cüce Bacaklıkları", "Sağlam cüce bacaklıkları.");
        AddTR(430, "Yubar'ın Alaşım Bacaklıkları", "Alaşımla güçlendirilmiş bacaklık.");
        AddTR(431, "Yubar'ın Plaka Bacaklıkları", "Ağır plaka bacaklıkları.");
        AddTR(432, "Yubar'ın Gnome Bacaklıkları", "Gnome zanaatkarlığının efsanevi bacaklıkları.");
        AddTR(433, "Geçici Savaş Çizmeleri", "Savaş alanı parçalarından monte edilmiş çizmeler. Yubar bulduğuyla idare eder.");
        AddTR(434, "Kabile Sargıları", "Yubar'ın halkının geleneksel sargıları. Onu atalarına bağlar.");
        AddTR(435, "Geyik Savaşçısı Bacaklıkları", "Geyik boynuzlarıyla süslenmiş çizmeler. Orman Kralının gücünü verir.");
        AddTR(436, "Diş Çiğneyici Çizmeler", "Diş şeklinde dikenleri olan vahşi çizmeler. Düşmanları ayakların altında ezerler.");
        AddTR(437, "Kanlı Çılgın Savaşçı Çizmeleri", "Yubar'ın öfkesini ateşleyen kan kırmızısı çizmeler. Acı güce dönüşür.");
        AddTR(438, "Ruh Çiğneyici", "Düşen düşmanların ruhlarını hasat eden çizmeler. Güçleri her savaşla büyür.");
        AddTR(439, "Evine Dönüş Şampiyonu Çizmeleri", "Yubar'ı ruhsal olarak eve getiren efsanevi çizmeler. Ataları yanında savaşır.");
        
        // Efsanevi Eşyalar (440-443)
        AddTR(440, "Rüya Yürüyücüsünün Kanatlı Miğferi", "Rüya yürüyücülerine uçma yeteneği veren efsanevi miğfer. En yakın düşmanlara otomatik saldırır.");
        AddTR(441, "Rüya Büyüsü Plaka Zırhı", "Tüm alemlerin rüya yürüyücülerini koruyan efsanevi zırh. En yakın düşmanlara otomatik nişan alır.");
        AddTR(442, "Derin Bacaklıkları", "Rüyaların en derin kısımlarında dövülmüş efsanevi bacaklıklar.");
        AddTR(443, "Su Üstünde Yürüme Çizmeleri", "Taşıyanın su üstünde yürümesini sağlayan efsanevi çizmeler. Tüm alemlerin rüya yürüyücülerinin aradığı hazine.");
        
        // Tüketilebilirler (444-447)
        AddTR(444, "Hayalci İksiri", "Uyanık dünyadan toplanan bitkilerle hazırlanan basit bir ilaç. En soluk rüya bile bir damla umutla başlar.");
        AddTR(445, "Hayal Özü", "Huzurlu uykuların anılarından damıtılmış. İçenler, unutulmuş rüyaların sıcaklığının yaralarını yıkadığını hisseder.");
        AddTR(446, "Berrak Canlılık", "Smoothie tarafından yıldız tozu ve kabus parçaları kullanılarak yaratılmış. Sıvı, bin uyuyan yıldızın ışığıyla parlar.");
        AddTR(447, "El'in İksiri", "El'in ışığıyla kutsanmış kutsal bir içecek. Vesper'in tarikatı tarifini kıskançlıkla korur, çünkü en karanlık kabusların verdiği yaraları bile iyileştirebilir.");
        
        // Universal Legendary Belt (448)
        AddTR(448, "Sonsuz Rüyaların Kemeri", "Kullanıcıyı rüyaların sonsuz döngüsüne bağlayan efsanevi kemer. Her fethedilen kabusla gücü büyür.");
        
        // Hero-Specific Legendary Belts (449-457)
        AddTR(449, "Aurena'nın Yaşam Dokuyucusu Kemeri", "Aurena'nın çalınan yaşam gücü araştırmasından dövülmüş bir kemer. Yaşam ve ölümü mükemmel dengeye dokur.");
        AddTR(450, "Bismuth'un Kristal Kemer", "Kör kızın görüşüyle nabız atan saf kristal kemer. Gözlerin göremediğini görür.");
        AddTR(451, "Lacerta'nın Kızıl Göz Kemeri", "Lacerta'nın efsanevi bakışını yönlendiren kemer. Hiçbir hedef onun uyanık gücünden kaçamaz.");
        AddTR(452, "Mist'in Düellocu Şampiyon Kemeri", "Astrid Evi'nin nihai kemeri. Mist'in onuru her ipliğe dokunmuştur.");
        AddTR(453, "Nachia'nın Dünya Ağacı Kemeri", "Dünya Ağacı'nın kendisinden yetiştirilmiş kemer. Doğanın öfkesi onun içinden akar.");
        AddTR(454, "Husk'un Kırılmaz Zinciri", "Kırılmaz kararlılıktan dövülmüş kemer. Husk'un kararlılığı onun gücüdür.");
        AddTR(455, "Shell'in Gölge Suikastçı Kemeri", "Mükemmel karanlığın kemeri. Shell'in manipülatörleri asla iradesini kıramadı.");
        AddTR(456, "Vesper'in Alacakaranlık Engizisyoncusu Kemeri", "El'in Tarikatı'nın en yüksek rütbeli kemeri. Kabusları yargılar ve karanlığı kovar.");
        AddTR(457, "Yubar'ın Atalar Şampiyon Kemeri", "Yubar'ın atalarını çağıran kemer. Onların gücü her liften akar.");
        
        // Universal Legendary Ring (458)
        AddTR(458, "Ebedi Rüyaların Yüzüğü", "Kullanıcıyı sonsuz rüya alemine bağlayan efsanevi yüzük. Gücü tüm sınırları aşar.");
        
        // Hero-Specific Legendary Rings (459-467)
        AddTR(459, "Aurena'nın Büyük Bilge Yüzüğü", "Ay Arkanum'un Büyük Bilgesi'nin yüzüğü. Aurena onu ikiyüzlülüklerinin kanıtı olarak aldı.");
        AddTR(460, "Bismuth'un Taş Kalp Yüzüğü", "Saf kristal büyünün yüzüğü. Kör kızın dönüştürülmüş özüyle nabız atar.");
        AddTR(461, "Lacerta'nın Kraliyet Celladı Yüzüğü", "Kraliyet Muhafızları'nın seçkin celladının yüzüğü. Lacerta sayısız savaşla kazandı.");
        AddTR(462, "Mist'in Astrid Mirası Yüzüğü", "Astrid Evi'nin atalar yüzüğü. Mist'in soyu onun metalinden akar.");
        AddTR(463, "Nachia'nın Orman Koruyucusu Yüzüğü", "Nachia'nın ormanının ruhları tarafından kutsanmış yüzük. Doğanın gücü onun özüdür.");
        AddTR(464, "Husk'un Sarsılmaz İrade Yüzüğü", "Kırılmaz iradeden dövülmüş yüzük. Husk'un kararlılığı onun gücüdür.");
        AddTR(465, "Shell'in Mükemmel Gölge Yüzüğü", "Mutlak karanlığın yüzüğü. Shell'in manipülatörleri asla gücünü kontrol edemedi.");
        AddTR(466, "Vesper'in Kutsal Alev Yüzüğü", "El'in Kutsal Alevi'nin saf özünü içeren yüzük. Tüm karanlığı kovar.");
        AddTR(467, "Yubar'ın Kabile Şampiyon Yüzüğü", "Yubar'ın atalarının gücünü yönlendiren yüzük. Onların öfkesi her darbeyi güçlendirir.");
        
        // Universal Legendary Amulet (468)
        AddTR(468, "Sonsuz Rüyaların Muskası", "Kullanıcıyı rüyaların sonsuz döngüsüne bağlayan efsanevi muska. Gücü tüm sınırları aşar.");
        
        // Hero-Specific Legendary Amulets (469-477)
        AddTR(469, "Aurena'nın Yaşam Dokuyucusu Muskası", "Aurena'nın çalınan yaşam gücü araştırmasından dövülmüş muska. Yaşam ve ölümü mükemmel dengeye dokur.");
        AddTR(470, "Bismuth'un Kristal Kalbi", "Kör kızın görüşüyle nabız atan saf kristal muska. Gözlerin göremediğini görür.");
        AddTR(471, "Lacerta'nın Kızıl Göz Muskası", "Lacerta'nın efsanevi bakışını yönlendiren muska. Hiçbir hedef onun uyanık gücünden kaçamaz.");
        AddTR(472, "Mist'in Düellocu Şampiyon Muskası", "Astrid Evi'nin nihai muskısı. Mist'in onuru her taşa dokunmuştur.");
        AddTR(473, "Nachia'nın Dünya Ağacı Muskası", "Dünya Ağacı'nın kendisinden yetiştirilmiş muska. Doğanın öfkesi onun içinden akar.");
        AddTR(474, "Husk'un Kırılmaz Muskası", "Kırılmaz kararlılıktan dövülmüş muska. Husk'un kararlılığı onun gücüdür.");
        AddTR(475, "Shell'in Gölge Suikastçı Muskası", "Mükemmel karanlığın muskısı. Shell'in manipülatörleri asla iradesini kıramadı.");
        AddTR(476, "Vesper'in Alacakaranlık Engizisyoncusu Muskası", "El'in Tarikatı'nın en yüksek rütbeli muskısı. Kabusları yargılar ve karanlığı kovar.");
        AddTR(477, "Yubar'ın Atalar Şampiyon Muskası", "Yubar'ın atalarını çağıran muska. Onların gücü her taştan akar.");
    }
    
    private static void AddTR(int id, string name, string desc)
    {
        _nameTR[id] = name;
        _descTR[id] = desc;
    }
}
