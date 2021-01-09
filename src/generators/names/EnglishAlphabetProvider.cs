using static LifeSim.Generation.CharGenerator;

namespace LifeSim.Generation
{
    public class EnglishAlphabetProvider
    {
        public MonogramProvider GetFemaleNameLetters(System.Random random)
        {
            return new MonogramProvider(random, new Monogram[] {
                new Monogram('a', 78.1793802111f, 18.0913223363f, 37.2413006f),
                new Monogram('b', 10.4140452138f, 3.71794180633f, 0.0268979700922f),
                new Monogram('c', 11.3844368001f, 5.57304146646f, 0.0703705955291f),
                new Monogram('d', 12.2277942632f, 2.3901341228f, 0.143149964499f),
                new Monogram('e', 54.0957177743f, 7.40038288027f, 18.238544275f),
                new Monogram('f', 1.64633929524f, 0.841636910668f, 0.0068248580831f),
                new Monogram('g', 6.72122347338f, 2.72810797728f, 0.0372786365884f),
                new Monogram('h', 18.6766542825f, 3.04073235889f, 7.55959133438f),
                new Monogram('i', 53.0583966975f, 2.48522332197f, 3.28654195338f),
                new Monogram('j', 6.16726293368f, 2.86088989267f, 0.0102086112503f),
                new Monogram('k', 11.2130124235f, 2.63793382296f, 0.0381389128173f),
                new Monogram('l', 51.1121651088f, 6.66501875976f, 2.54309123631f),
                new Monogram('m', 18.8982040873f, 9.51620358953f, 0.284980838781f),
                new Monogram('n', 41.7751283819f, 3.05621733101f, 13.567187f),
                new Monogram('o', 18.1628399668f, 1.17622701199f, 0.121126893038f),
                new Monogram('p', 4.81416312902f, 2.08502282026f, 0.00430138114481f),
                new Monogram('q', 0.41258847941f, 0.160871654816f, 0.0044734363906f),
                new Monogram('r', 30.0121700411f, 3.33351303548f, 2.27829821303f),
                new Monogram('s', 21.0232584281f, 7.83103716049f, 1.23868306621f),
                new Monogram('t', 19.6874111765f, 2.02732696117f, 0.943952430166f),
                new Monogram('u', 5.66073229007f, 0.0346404561529f, 0.0439887911743f),
                new Monogram('v', 7.46226541701f, 1.69990582843f, 0.013535012669f),
                new Monogram('w', 0.66940960963f, 0.288020481456f, 0.174464019233f),
                new Monogram('x', 1.45283449547f, 0.152727706515f, 0.0839056081981f),
                new Monogram('y', 16.1148571999f, 0.606609444915f, 11.9682202491f),
                new Monogram('z', 4.49488594458f, 1.59931086139f, 0.0709441130151f),
            });
        }

        public MonogramProvider GetMaleNameLetters(System.Random random)
        {
            return new MonogramProvider(random, new Monogram[] {
                new Monogram('a', 70.6810707752f, 10.3684018523f, 1.43004359045f),
                new Monogram('b', 20.6242460876f, 6.90801004418f, 1.83186389768f),
                new Monogram('c', 19.6166064424f, 8.5535800352f, 1.28044607646f),
                new Monogram('d', 21.7057525011f, 6.14255167878f, 0.2439627098f),
                new Monogram('e', 51.9872929986f, 5.55625084878f, 6.78200249773f),
                new Monogram('f', 1.53762172439f, 0.858867606999f, 0.129839351762f),
                new Monogram('g', 6.58825534362f, 3.39004263183f, 0.200936781869f),
                new Monogram('h', 21.0040448313f, 3.16945686356f, 5.21354525871f),
                new Monogram('i', 34.9117140339f, 2.10123528456f, 2.48441581612f),
                new Monogram('j', 10.7687150005f, 10.8770973076f, 0.0790859054005f),
                new Monogram('k', 6.252835243f, 0.49804415497f, 1.80294561712f),
                new Monogram('l', 29.6210001039f, 5.79297599996f, 6.80149437744f),
                new Monogram('m', 18.1663253812f, 6.89320207381f, 0.90588777257f),
                new Monogram('n', 52.159844065f, 3.2692036289f, 32.0549181048f),
                new Monogram('o', 35.1085500651f, 1.34398108329f, 36.13952138382f),
                new Monogram('p', 3.77972045662f, 1.56755187849f, 0.133141254883f),
                new Monogram('q', 0.483409268229f, 0.195344849164f, 0.0178409281543f),
                new Monogram('r', 35.1831091678f, 4.2618515688f, 8.85091108561f),
                new Monogram('s', 24.7227333367f, 3.65265044296f, 6.43743293009f),
                new Monogram('t', 20.2958931249f, 3.60690310778f, 2.50108510123f),
                new Monogram('u', 10.019198969f, 0.1390527266f, 0.140756934662f),
                new Monogram('v', 6.77209678837f, 0.629758135596f, 0.148905179461f),
                new Monogram('w', 5.25662976879f, 2.07854801473f, 1.54087037101f),
                new Monogram('x', 3.90780234382f, 0.460775254899f, 0.900247909017f),
                new Monogram('y', 16.605590335f, 0.409595756522f, 5.75713437414f),
                new Monogram('z', 2.41310536f, 1.27506716976f, 0.190764789996f),
            });
        }

        public MonogramProvider GetOtherNameLetters(System.Random random)
        {
            return new MonogramProvider(random, new Monogram[] {
                new Monogram('a', 74.2914138476f, 14.0868923608f, 19.6727200579f),
                new Monogram('b', 20.5230369718f, 4.85351932756f, 0.962795171168f),
                new Monogram('c', 15.6529187749f, 7.11848764998f, 0.69780972797f),
                new Monogram('d', 17.1422330629f, 4.33580912653f, 1.23244740677f),
                new Monogram('e', 53.0024734f, 6.44417756897f, 14.298185391f),
                new Monogram('f', 1.5899678875f, 0.850571240243f, 0.070609397484f),
                new Monogram('g', 6.65227785033f, 3.07132929854f, 0.122137413012f),
                new Monogram('h', 19.8834351237f, 2.58896521748f, 6.34313737131f),
                new Monogram('i', 43.6491168579f, 2.28612076609f, 2.87062961105f),
                new Monogram('j', 11.6642472063f, 0.5359051686f, 0.045922341813f),
                new Monogram('k', 10.7151486567f, 1.00986211976f, 0.953213058955f),
                new Monogram('l', 39.9687297295f, 6.21285378221f, 2.75112603626f),
                new Monogram('m', 18.5187159086f, 8.15614479869f, 1.64395356293f),
                new Monogram('n', 47.1597321537f, 3.16665336585f, 25.2273542518f),
                new Monogram('o', 26.9494006485f, 1.26320957681f, 2.20471423352f),
                new Monogram('p', 4.27779178429f, 1.81670772431f, 0.0711064522961f),
                new Monogram('q', 0.049309936043f, 0.178746433287f, 0.0114046465236f),
                new Monogram('r', 32.6933660579f, 3.81486806923f, 5.68627943704f),
                new Monogram('s', 22.9414820131f, 5.66449186777f, 3.93429929493f),
                new Monogram('t', 16.632503353f, 2.84635676772f, 1.75134501651f),
                new Monogram('u', 7.92065127435f, 0.0887795122843f, 0.0941642727494f),
                new Monogram('v', 7.1044044303f, 1.14502098814f, 0.0837261216939f),
                new Monogram('w', 3.04794010821f, 1.21643119615f, 0.882962645502f),
                new Monogram('x', 2.72576574746f, 0.31245417776f, 0.507189207504f),
                new Monogram('y', 6.0731101355f, 0.504455406037f, 8.74769525349f),
                new Monogram('z', 3.41545691349f, 1.43118648917f, 0.13307261888f),
            });
        }

        public readonly string[] syllabes = new string[] {
            // A
            "ache","act","acts","add","adds","adv","aft","age","aid","aim","aimed","aims","air","airs","aisle",
            "ale","all","alms","alps","ann","anne","ant","ape","apt","arch","arched","are","ark","arm","arms",
            "art","arts","ash","ask","asked","asks","asp","ate","aug","aught","aunt","aux","awe","awed",
            "axe","aye",

            // B
            "babe","back","backed","backs","bad","bade","bag","bags","bait","bake","baked",
            "band","bands","bang","bank","banks","bar","bare","barge","bark","bars","bart","base","based","bass","bat",
            "bath","bathe","bay","beach","beads","beak","beam","beams","beans","bear","beard","bears","beast","beasts","beat",
            "bed","beds","bee","beef","been","beer","bees","beg","begged","bell","bells","ben","bench","bent","berth",
            "best","bet","beth","bid","bids","big","bill","bills","bin","bind","birch","bird","birds","birth","bit",
            "bite","bits","black","blade","blame","blamed","blanche","blank","blast","blaze","blazed","bleak","bless","blest","blew",
            "blind","bliss","block","blocker","blocks","blood","bloom","blot","blow","blown","blows","blue","bluff","blunt","blush",
            "blusher","board","boards","boat","boats","bob","boil","boiled","bold","bolt","bolts","bond","bonds","bone","bones",
            "book","books","boon","boot","boots","bore","bored","born","borne","boss","both","bough","boughs","bought","bound",
            "bow","bowed","bowl","bows","box","boxes","boy","boys","brace","brain","brains","brake","branch","brand","brass",
            "brave","breach","bread","breadth","break","breaks", "breath","breathe","breathed","bred","breed","breeze","bribe",
            "brick","bricks","bride","bridge","brief","brig","bright","brim","bring","brings","brink","brisk","bronze","brood","brook",
            "brown","brows","bruce","brush","brusher","brute","buck","bud","bug","build","built","bulk","bull","bunch","burke",
            "burn","burned","burns","burnt","burst","bush","bust","but","buy",
            
            // C
            "cage","cake","calf","call","calls","calm",
            "calves","camp","can","cane","canst","cap","cape","caps","car","card","care","cared","cares","carl","cars",
            "cart","carts","carved","case","cash","cast","caste","cat","catch","cats","caught","cause","cave","cease","ceased",
            "cell","cells","cent","cents","chain","chains","chair","chairs","chalk","chance","chancer","change","chant","chap","chaps",
            "charge","charm","charmer","charms","chart","chase","chasm","chaste","chat","cheap","cheat","check","checker","checks","cheer",
            "cheerer","cheers","cheese","cheque","chest","chief","chiefs","child","choice","choir","choked","choose","chose","church","clad",
            "claim","claimer","claims","clan","clapper","clare","clash","clasp","clasper","class","claude","clause","clay","clean","clear",
            "clearer","cleft","clerk","clerks","click","cliff","cliffs","climber","cling","clock","close","closed","cloth","clothe","clother",
            "clothes","cloud","clouds","club","clubs","clue","clump","clung","clutch","clutched","coach","coal","coarse","coast","coasts",
            "coat","coats","cock","cocked","code","codes","coil","coin","cold","comb","comes","con","cook","cooked","cool",
            "cor","cord","cords","core","cork","corn","corps","corpse","cost","costs","cot","couch","cough","count","counts",
            "course","court","courts","cow","cows","crack","cracker","craft","craig","crash","crawl","cream","creed","creek","creep",
            "crept","crest","crew","cried","cries","crime","crimes","crisp","crop","crops","cross","crosser","crow","crowd","crowds",
            "crown","crowner","crowns","crude","cruise","crush","crusher","crust","cry","cup","cups","curb","cure","cured","curl",
            "curse","curve","curved","cut",
            
            // D
            "dad","dam","dame","damn","damned","damp","dan","dance","danced","dare","dark",
            "dart","dash","dashed","date","dates","dave","dawn","dawned","days","dazed","dead","deaf","deal","deals","dealt",
            "dean","dear","death","debt","debts","deck","deed","deeds","deem","deemed","deep","deer","def","del","den",
            "dense","depth","depths","desk","deuce","dey", "did","didst","die","dies","dig","dim","din","dine",
            "dined","dip","dipped","dire","dirt","dis","dish","disk","ditch","dock","doe","does","dog","dogs","doll",
            "dome","don","done","doom","doomed","door","doors","dose","dost","dot","doth","doubt","doubts","dough","dove",
            "draft","drag","dragger","drain","drainer","drank","draught","draw","drawn","draws","dread","dream","dreamer","dreams","dreamt",
            "dress","dresser","drew","dried","drift","drink","drinks","drive","drives","drooper","drop","dropper","drops","drove","drown",
            "drug","drugs","drum","drums","drunk","dry","duck","ducks","due","duke","dull","dumb","durst","dusk","dust",
            "dutch","dwell","dwelt","dye",
            
            // E
            "each","ear","earl","earn","earned","ears","earth","ease","east","eat","eats",
            "ebb","edge","egg","eggs","eight","eighth","ein","else","email","end","ends","esp","esq","ety","eve",
            "eye","eyed","eyes",
            
            // F
            "face","faced","fact","facts","fade","fail","failed","fain","faint","fair","faith","fall",
            "falls","false","fame","fan","far","farce","fare","farm","farms","fast","fat","fate","fault","faults","fear",
            "feared","fears","feast","feat","feb","fed","fee","feed","feel","feels","fees","feet","fell","felt","fence",
            "fetch","few","field","fields","fiend","fierce","fifth","fight","file","files","fill","filled","film","filth","find",
            "finds","fine","fire","fires","firm","fish","fist","fit","fits","five","fix","fixed","flag","flags","flame",
            "flames","flank","flash","flasher","flat","fled","flee","fleet","flesh","flew","flies","flight","flights","fling","flint",
            "float","flock","flood","floods","floor","flour","flow","flowed","flown","flows","flung","flush","flusher","flute","fly",
            "foam","foe","foes","fog","fold","folds","folk","folks","fond","food","fool","fools","foot","for","force",
            "forced","ford","fore","forged","fork","form","formed","forms","fort","forth","fought","foul","found","four","fourth",
            "fowl","fox","frail","frame","framed","frank","fraud","fred","free","freed","freight","french","fresh","fret","fried",
            "friend","friends","fright","fringe","fro","frock","frog","from","front","frost","frown","fruit","fruits","full","fun",
            "fund","funds","fur","furs","fuss",
            
            // G
            "gain","gained","gains","gait","gal","gale","game","games","gang","gap",
            "garb","gas","gasp","gasped","gate","gates","gaunt","gave","gay","gaze","gazed","gear","geese","gem","germ",
            "gets","ghost","ghosts","gift","gifts","gilt","gin","girl","git","gives","glad","glance","glancer","glare","glass",
            "gleam","glee","glen","glide","glimpse","globe","gloom","glove","glow","goal","goat","gods","goes","gold","gone",
            "good","goods","goose","gorge","got","gout","gown","grace","grade","grain","grand","grant","grapes","grasp","grasper",
            "grass","grate","grave","gray","greece","greed","greek","greeks","green","greet","grew","grey","grief","grieve","griever",
            "grim","grin","grind","grip","groan","groom","ground","grounds","group","groups","grove","groves","grow","growl","growler",
            "grown","grows","growth","grudge","guard","guards","guess","guesser","guest","guests","guide","guides","guilt","guise","gulf",
            "gum","gun","guns","guy",
            
            // H
            "had","hail","hair","half","hall","halt","ham","hand","hands","hang","hanged",
            "hans","hard","hare","harm","harp","harsh","hart","has","hast","haste","hat","hate","hath","hats","haul",
            "haunt","hawk","hay","haze","head","heads","heal","health","heap","heaped","hear","heard","hears","heart","hearth",
            "hearts","heat","heath","heave","hedge","heed","heel","heels","height","heights","heir","held","hell","helm","help",
            "helped","hem","hen","hence","her","herb","herd","here","hers","hid","hide","hides","high","hill","hills",
            "him","hint","hints","hire","hired","hit","hoarse","hold","holds","hole","holes","holmes","holt","home","homes",
            "hon","hood","hook","hope","hoped","hopes","horn","horns","horse","host","hosts","hot","hound","hounds","hour",
            "hours","house","howl","huge","hull","hum","hung","hunt","hurled","hurt","hurts","hush","hushed","hut","hymn",

            // I
            "ice","ill","ills","inch","ink","isle","its",
            
            // J
            "jack","jail","jake","james","jan","jane","jar","jaw",
            "jaws","jay","jean","jeff","jerk","jest","jew","jews","jim","job","jobs","joe","john","join","joined",
            "joint","joke","jokes","jones","jove","joy","joys","juan","judge","judged","jug","juice","jump","jumped","june",

            // K
            "karl","kate","keen","keep","keeps","keith","ken","kent","kept","key","keys","khan","kick","kid","kill",
            "killed","kin","kind","kinds","king","kings","kirk","kiss","kissed","knave","knee","kneel","knees","knelt","knew",
            "knife","knight","knights","knit","knives","knock","knocker","knot","knots","known","knows",
            
            // L
            "lace","lack","lacked","lad",
            "lads","laid","lain","lake","lakes","lamb","lambs","lame","lamp","lamps","lance","land","lands","lane","lap",
            "lapse","large","lark","las","lash","last","late","laugh","laugher","launch","launched","law","lawn","laws","lay",
            "lays","lead","leads","leaf","league","leagues","lean","leaned","leap","leaped","leaps","leapt","learn","learnt","lease",
            "least","leave","leaves","led","ledge","lee","left","leg","legs","lend","length","lent","less","lest","let",
            "lets","lid","lids","lie","lied","lies","lieu","lift","lights","like","liked","likes","limb","limbs","lime",
            "limp","line","lined","lines","link","linked","links","lip","lips","list","lists","lit","live","lived","lives",
            "load","loaf","loan","lock","locke","locked","locks","lodge","lodged","log","login","logs","lone","long","longed",
            "look","looks","loose","lord","lords","lore","lose","loss","lost","lot","lots","loud","love","loved","loves",
            "low","luck","luke","lump","lunch","lungs",
            
            // M
            "mac","mad","made","maid","maids","mail","main","maine","make",
            "makes","male","males","man","mane","manned","map","maps","march","marcher","mark","marked","marks","mars","marsh",
            "mask","mass","mast","mat","match","mate","mates","matt","max","may","meal","mean","meant","meat","meats",
            "meek","meet","meets","melt","men","mend","mere","mess","met","mice","midst","mien","might","mike","mild",
            "mile","miles","milk","mill","mills","mind","minds","mine","mines","mirth","missed","mist","mix","mixed","moan",
            "mob","mock","mode","moist","mon","monk","monks","month","months","mood","moon","moor","moore","moss","most",
            "mould","mound","mount","mourn","mouse","mouth","mouths","move","moved","moves","mud","mule","muse","mute","myth",
            
            // N
            "nail","nails","name","named","names","nan","naught","nay","near","neat","neck","ned","need","needs","nerve",
            "nerves","ness","nest","net","new","news","next","nice","nick","niece","nigh","night","nights","nile","nine",
            "ninth","nod","noise","non","none","nook","noon","nor","north","nose","not","note","notes","nought","noun",
            "nous","nov","now","nun","nurse",
            
            // O
            "oak","oar","oars","oath","oats","obs","odd","odds","off","oft",
            "oil","old","ole","once","one","ones","orleans","ought","ounce","our","ours","owe","owed","owl","own",
            "owned","owns",
            
            // P
            "pack","packed","page","paid","pail","pain","pains","paint","pair","pairs","pale","palm","pan",
            "pang","parcher","park","part","parts","pass","past","paste","pat","patch","path","paths","paul","pause","paused",
            "pay","pays","peace","peak","peaks","pearl","peas","peep","peer","peers","peg","pen","per","perch","pet",
            "phase","phrase","pick","pie","piece","pier","pierce","piercer","pierre","pig","pigs","pile","piled","piles","pin",
            "pinch","pine","pink","pins","pint","pipe","pipes","pit","pitch","pitcher","place","placed","plague","plain","plains",
            "plan","plane","plank","planner","plans","plant","plants","plate","play","played","plays","plea","plead","please","pleaser",
            "pledge","pledger","plight","plot","plough","pluck","plucker","plump","plunge","plunger","plus","point","points","pole","poles",
            "pomp","pond","pool","pools","poor","pope","porch","pork","port","ports","pose","post","posts","pot","pots",
            "pound","pounds","pour","poured","praise","praiser","pray","prayed","prayer","prayers","preach","preached","press","presser","prey",
            "price","pride","priest","priests","prime","prince","print","prints","prize","pro","prompt","prone","proof","proofs","prose",
            "proud","prove","proved","proves","psalms","pseud","pub","puff","puffed","pull","pulled","pulse","pump","punch","pure",
            "purse","push","pushed","put",
            
            // Q
            "quaint","quart","quay","queen","queer","quest","quick","quit","quod","quote","quoth",

            // R
            "race","rack","raft","rag","rage","rags","raid","rail","rain","rains","raise","raised","ralph","ram","ran",
            "ranch","rang","range","rank","ranks","rare","rash","rat","rate","rates","rats","raw","ray","rays","reach",
            "reacher","read","reads","realm","reap","rear","reared","red","reed","reef","reign","reigner","reigns","rein","reins",
            "rent","rest","rests","rev","rhine","rhyme","rhythm","rice","rich","rid","ride","rides","ridge","right","rights",
            "ring","rings","ripe","rise","risk","road","roads","roar","roared","roast","rob","robbed","robe","robes","rock",
            "rocks","rod","rode","rogue","role","roll","rolled","rolls","roof","roofs","room","rooms","root","roots","rope",
            "ropes","rose","rot","rough","round","rounds","rouse","roused","rout","route","row","rows","rub","rubbed","rude",
            "rue","rug","rule","ruled","rules","run","rung","runs","rush","rushed","ruth",
            
            // S
            "sack","sad","safe","said",
            "sail","sailed","sails","saint","saints","sake","sale","sales","sam","same","sand","sane","sang","sank","sap",
            "sate","sauce","saul","save","saved","saw","say","says","scale","scales","scant","scar","scarce","scare","scared",
            "scarf","scene","scenes","scent","scheme","schemes","school","schools","scope","score","scorn","scorner","scot","scotch","scots",
            "scourge","scout","scrap","scrape","scraps","scratch","scream","screen","screw","scrub","sea","seal","sealed","search","searched",
            "seas","seat","seats","sect","seed","seeds","seek","seem","seems","seen","sees","seine","seize","seized","self",
            "sell","send","sends","sense","sent","serve","served","serves","set","sets","sex","sexes","sez","shade","shades",
            "shaft","shake","shalt","sham","shame","shape","shaped","shapes","share","shared","shares","sharp","shaw","shawl","she",
            "shed","sheep","sheer","sheet","sheets","shelf","shell","shells","shelves","shew","shield","shift","shine","ship","ships",
            "shirt","shock","shocker","shoe","shoes","shone","shook","shoot","shop","shops","shore","shores","short","shot","should",
            "shout","shouts","show","showed","shown","shows","shrank","shrewd","shriek","shrill","shrine","shrink","shrug","shrugged","shrunk",
            "shun","shut","shy","sic","sick","side","sides","siege","sigh","sighed","sight","sign","signed","signs","silk",
            "sin","since","sing","sings","sink","sins","sir","sire","sit","site","sites","sits","six","sixth","size",
            "sketch","skies","skill","skin","skirt","skirts","skull","sky","slate","slave","slaves","slay","sleep","sleeve","slept",
            "slew","slice","slid","slide","slight","slim","slip","slipper","slips","slope","slow","sly","small","smart","smasher",
            "smelt","smile","smiled","smiles","smith","smoke","smoked","smooth","smote","snakes","snap","snapper","snare","snatch","snatched",
            "sneer","snow","snuff","snug","soap","sob","soft","soil","soiled","sol","sold","sole","solve","some","son",
            "song","songs","sons","soon","soothe","soother","sore","sort","sorts","sought","soul","souls","sound","sounds","soup",
            "sour","source","sous","south","sow","sown","space","spade","spake","span","spare","spared","spark","speak","speaks",
            "spear","speck","speech","speed","spell","spend","spent","sphere","spies","spin","spit","spite","splash","split","spoil",
            "spoiler","spoils","spoke","sponge","spoon","sport","sports","spot","spots","spouse","spray","spread","spring","springs","sprung",
            "spun","spur","spy","square","squeeze","squire","staff","stage","staid","stain","stainer","stair","stairs","stake","stale",
            "stalk","stall","stamp","stamper","stand","stands","star","stare","stared","stars","start","starve","state","states","stay",
            "stayed","stead","steal","steam","steed","steel","steep","steer","stem","step","stepper","steps","stern","steve","stick",
            "sticks","stiff","still","sting","stir","stirrer","stock","stocks","stone","stones","stood","stool","stoop","stooper","stop",
            "stopper","store","stores","storm","storms","stout","stove","straight","strain","strained","strait","strand","strange","straw","stray",
            "streak","stream","streams","street","streets","strength","stress","stretch","strict","stride","strife","strike","strikes","string","strip",
            "stripped","strive","strode","stroke","strokes","stroll","strong","strove","struck","stuff","stuffer","stump","stunner","style","sub",
            "sue","suit","suite","suits","sum","sums","sun","sunk","sup","sure","surf","swam","swamp","swarm","sway",
            "swear","sweat","sweep","sweet","swell","sweller","swift","swim","swine","swing","swiss","sword","swords","sworn","swung",

            // T
            "tact","tail","take","takes","tale","talk","talked","talks","tall","tame","tank","tap","tapped","task","taste",
            "tastes","taught","tax","taxes","tea","teach","team","tear","tears","ted","teeth","tell","tells","tempt","ten",
            "tend","tends","tense","tent","tenth","tents","term","termed","terms","test","text","texts","than","thank","thanker",
            "thanks","thee","theft","theirs","theme","then","thence","they","thick","thief","thieves","thigh","thin","thine","thing",
            "think","thinks","third","thirst","this","thorn","thou","thought","thoughts","thread","threads","threat","threats","three","threw",
            "thrice","thrill","thrilled","throat","throne","throng","through","throw","thrown","throws","thrust","thumb","thus","thy","tide",
            "tie","tied","tight","till","tim","time","times","tin","tinge","tint","tints","tip","tips","tire","tired",
            "toast","toe","told","tom","tomb","ton","tone","tones","tongue","tongues","tons","too","took","tools","tooth",
            "top","tops","tore","torn","toss","tossed","touch","toucher","tough","tour","tout","town","towns","toy","trace",
            "traced","track","tracks","tract","trade","trades","trail","train","trainer","trains","trait","traits","tramp","trance","trap",
            "tray","tread","treat","tree","trees","trench","tribe","tribes","trick","tricks","tried","tries","trim","trip","trod",
            "troop","troops","trot","trout","troy","truce","true","trunk","trunks","trust","truth","truths","try","tub","tube",
            "tune","turf","turk","turn","turned","turns","twain","twelfth","twelve","twice","twin","twins","twist","type","types",

            // U
            "unit","urge","urged","use","used",
            
            // V
            "vague","vain","val","valve","van","vast","vault","veil","vein","veins",
            "vent","vera","verb","verge","verse","vest","vexed","vice","view","viewed","views","vile","vine","vines","voice",
            "void","vol","vote","votes","vow","vowed",
            
            // W
            "wad","wade","wage","wail","waist","wait","wake","waked","wales",
            "walk","walked","walks","wall","walls","walt","wan","wand","want","wants","war","ward","ware","wares","warm",
            "warmth","warn","warned","wars","wash","washed","wast","waste","watch","watcher","wave","waved","waves","ways","weak",
            "wealth","wear","wears","web","wed","wee","weed","week","weeks","weep","weigh","weigher","weight","weird","well",
            "wells","welsh","wench","went","wept","were","wert","west","wet","whale","wharf","what","wheat","wheel","wheeler",
            "wheels","when","whence","whig","whilst","whim","whip","whipper","whirl","whit","white","whites","who","whom","whose",
            "wid","width","wig","wild","will","wilt","win","wind","winds","wine","wines","wing","winged","wings","wink",
            "wins","wipe","wiped","wire","wires","wise","wish","wished","wit","witch","wits","wives","woe","woke","wolf",
            "wolves","won","wont","wood","woods","wool","word","words","wore","work","worked","workmen","works","world","worlds",
            "worm","worms","worn","worse","worst","worth","wound","wounds","wrap","wrapper","wrath","wreath","wreck","wrecker","wretch",
            "wright","wrist","writ","write","writes","wrong","wrongs","wrote","wrought","wrung",
            
            // Y
            "yacht","yale","yard","yards","yarn", "yea","year","yell","yes","yield","yoke","yon","yore","you","yours","youth",
            
            // Z
            "zeal","zeus","zone"
        };
    }
}