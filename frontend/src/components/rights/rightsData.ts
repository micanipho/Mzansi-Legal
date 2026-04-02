import { R } from "@/styles/theme";

export interface PublicFaqCitation {
  id: string;
  actName: string;
  sectionNumber: string;
  excerpt: string;
  relevanceScore: number;
}

export interface PublicFaqItem {
  id: string;
  conversationId: string;
  questionId: string;
  answerId: string;
  categoryId?: string | null;
  categoryName: string;
  topicKey: string;
  title: string;
  summary: string;
  explanation: string;
  sourceQuote?: string | null;
  primaryCitation: string;
  languageCode: string;
  publishedAt: string;
  citations: PublicFaqCitation[];
}

export interface PublicFaqListResponse {
  items: PublicFaqItem[];
  totalCount: number;
}

export interface RightsAcademyLesson {
  id: string;
  documentId: string;
  topicKey: string;
  categoryName: string;
  title: string;
  lawShortName: string;
  lawTitle: string;
  summary: string;
  explanation: string;
  sourceQuote?: string | null;
  primaryCitation: string;
  askQuery: string;
  citations: PublicFaqCitation[];
}

export interface RightsAcademyTrack {
  topicKey: string;
  categoryName: string;
  sortOrder: number;
  lessons: RightsAcademyLesson[];
}

export interface RightsAcademyResponse {
  tracks: RightsAcademyTrack[];
  totalLessons: number;
}

export interface RightsAcademyProgressResponse {
  exploredLessonIds: string[];
}

type SupportedRightsLocale = "en" | "zu" | "st" | "af";

type RightsLessonOverride = Partial<
  Pick<RightsAcademyLesson, "title" | "summary" | "explanation" | "askQuery" | "sourceQuote" | "lawTitle" | "primaryCitation">
>;

export const RIGHTS_PROGRESS_STORAGE_KEY = "ml_rights_progress";

export const RIGHTS_RADIUS_CYCLE = [
  R.o1,
  R.o2,
  R.o3,
  R.o4,
  "28px 20px 34px 18px",
  "20px 34px 18px 30px",
] as const;

export const TOPIC_LABEL_KEYS: Record<string, string> = {
  legal: "legal",
  employment: "employment",
  housing: "housing",
  consumer: "consumer",
  debtCredit: "debtCredit",
  tax: "tax",
  privacy: "privacy",
  contracts: "contracts",
  insurance: "insurance",
  safety: "safety",
  family: "family",
  criminal: "criminal",
};

export const TOPIC_ORDER = [
  "employment",
  "housing",
  "consumer",
  "debtCredit",
  "tax",
  "privacy",
  "safety",
  "insurance",
  "contracts",
  "family",
  "criminal",
  "legal",
] as const;

export function getRightCardRadius(index: number): string {
  return RIGHTS_RADIUS_CYCLE[index % RIGHTS_RADIUS_CYCLE.length];
}

export function getTopicLabelKey(topicKey: string): string | null {
  return TOPIC_LABEL_KEYS[topicKey] ?? null;
}

export function sortTopicKeys(topicKeys: string[]): string[] {
  return [...topicKeys].sort((left, right) => {
    const leftIndex = TOPIC_ORDER.indexOf(left as (typeof TOPIC_ORDER)[number]);
    const rightIndex = TOPIC_ORDER.indexOf(right as (typeof TOPIC_ORDER)[number]);

    const normalizedLeft = leftIndex === -1 ? Number.MAX_SAFE_INTEGER : leftIndex;
    const normalizedRight = rightIndex === -1 ? Number.MAX_SAFE_INTEGER : rightIndex;

    return normalizedLeft - normalizedRight || left.localeCompare(right);
  });
}

export function normalizeRightsProgressIds(ids: string[]): string[] {
  const seen = new Set<string>();

  return ids.filter((id): id is string => typeof id === "string" && id.trim().length > 0).filter((id) => {
    const normalized = id.trim();
    if (seen.has(normalized)) {
      return false;
    }

    seen.add(normalized);
    return true;
  });
}

export function readRightsProgress(): string[] {
  if (typeof window === "undefined") return [];

  try {
    const stored = window.localStorage.getItem(RIGHTS_PROGRESS_STORAGE_KEY);
    const parsed = stored ? (JSON.parse(stored) as unknown) : [];
    return Array.isArray(parsed) ? normalizeRightsProgressIds(parsed) : [];
  } catch {
    return [];
  }
}

export function writeRightsProgress(ids: string[]): void {
  if (typeof window !== "undefined") {
    window.localStorage.setItem(RIGHTS_PROGRESS_STORAGE_KEY, JSON.stringify(normalizeRightsProgressIds(ids)));
  }
}

export function mergeRightsProgressIds(...collections: Array<string[] | null | undefined>): string[] {
  return normalizeRightsProgressIds(collections.flatMap((collection) => collection ?? []));
}

const RIGHTS_ACADEMY_LOCALE_OVERRIDES: Record<Exclude<SupportedRightsLocale, "en">, Record<string, RightsLessonOverride>> = {
  zu: {
    "academy-legal-equality-dignity": {
      title: "Ukulingana nesithunzi kuza kuqala",
      summary: "UMthethosisekelo uvikela ukuphathwa ngokulingana nesithunzi emsebenzini, ekuqashiseni, ezikoleni nasezinsizeni.",
      explanation: "Izigaba 9 no-10 zoMthethosisekelo zibeka isisekelo sokuthi abantu kufanele baphathwe kanjani. Uma umthetho, isinqumo noma inkontileka ikubeka eceleni ngenxa yokuthi ungubani noma ikwehlisa isithunzi sakho, lokho kungaba yinto ephikisana noMthethosisekelo futhi kungase kusekele isikhalazo noma inselelo enkantolo.",
      askQuery: "Chaza ukuthi uMthethosisekelo uvikela kanjani isithunzi sami nokulingana kwami empilweni yansuku zonke.",
    },
    "academy-legal-courts-and-fair-process": {
      title: "Ungaphikisa izinqumo ezingalungile",
      summary: "Unelungelo lokuthi impikiswano yezomthetho ilalelwe ngokulingana yinkantolo noma isigungu esizimele.",
      explanation: "Isigaba 34 sibalulekile ngoba sisho ukuthi akumele wamukele nje ukuxoshwa, ukuvinjelwa, noma esinye isinqumo esingalungile. Kumele kube nenqubo efanele, futhi uhlelo lwezomthetho kumele luhlale luvulekile kuwe uma amalungelo akho ethintekile.",
      askQuery: "Isigaba 34 sisho ukuthini uma ngifuna ukuphikisa isinqumo esingalungile?",
    },
    "academy-employment-written-terms": {
      title: "Imigomo yomsebenzi wakho kufanele ibhalwe phansi",
      summary: "Abasebenzi kufanele banikwe imigomo ebhaliwe ecacisa iholo, amahora, ikhefu nezinye izimo zomsebenzi.",
      explanation: "I-BCEA ifuna umqashi anikeze umsebenzi imininingwane ebhaliwe ngomsebenzi kanye nolwazi olucacile lweholo. Lokho kubalulekile ngoba abasebenzi bavame ukucindezelwa ngezinguquko noma izikweletu ezingacaci. Imibhalo ibasiza ukuqhathanisa okuthembisiwe nalokho okwenzekayo futhi iba ubufakazi obubalulekile lapho kunengxabano.",
      askQuery: "Yiluphi ulwazi olubhaliwe umqashi okufanele anginike lona ngaphansi kwe-BCEA?",
    },
    "academy-employment-dismissal": {
      title: "Ukuxoshwa kufanele kube nobulungisa",
      summary: "Umsebenzi unelungelo lokungaxoshwa ngokungafanele futhi angaphikisa ukuxoshwa ngezinqubo zabasebenzi.",
      explanation: "Umthetho Wezobudlelwano Emsebenzini uthi umsebenzi unelungelo lokungaxoshwa ngokungafanele. Ubulungisa buvamise ukufuna isizathu esifanele nenqubo efanele. Uma uxoshwa ngokuphazima kweso, ngaphandle kokulalelwa, noma ngesizathu esibuthakathaka, ungase ukwazi ukuya e-CCMA noma esigungwini esifanele.",
      askQuery: "Ngingazi kanjani ukuthi ukuxoshwa kwami bekungafanele ngaphansi kwe-LRA?",
    },
    "academy-housing-eviction-court-order": {
      title: "Awukwazi ukuxoshwa ngaphandle kwenqubo yenkantolo",
      summary: "Umnikazi wendawo noma indlu akakwazi ukukususa ngokusemthethweni ngaphandle komyalelo wenkantolo nenqubo enobulungisa.",
      explanation: "Izingxabano zezindlu zivame ukuqala ngezinsongo, ukuvalelwa ngaphandle, noma ukunqanyulwa kwezinsiza, kodwa umthetho uqinile. I-PIE nesigaba 26 soMthethosisekelo zifuna inqubo yenkantolo ngaphambi kokuxoshwa. Inkantolo kumele ibheke ubulungisa nokufaneleka, ikakhulukazi uma kuthinteka izingane, ubudala, ukukhubazeka noma ukuhlala isikhathi eside.",
      askQuery: "Ingabe umnikazi wendlu angangixosha noma angivalele ngaphandle ngaphandle komyalelo wenkantolo?",
    },
    "academy-housing-lease-and-deposit": {
      title: "Imigomo yokuqasha nedipozithi kufanele kuphathwe ngobulungisa",
      summary: "Abaqashi banelungelo lesivumelwano esibhaliwe uma besicela kanye nokuphathwa kahle kwedipozithi, ukuhlolwa nokuxazululwa kwezinkinga.",
      explanation: "Umthetho Wezindlu Zokuqasha unikeza ukuvikeleka okusebenzayo ngemigomo yokuqasha, ukuhlolwa kwendawo, amadipozithi kanye nezikhalo ngezindlela ezingalungile. Usiza umqashi ukuthi aphikisane uma umnikazi enqaba ukubhala isivumelwano, ebamba idipozithi ngendlela engafanele, noma esebenzisa imigomo engaqondakali ukuze abeke wonke umthwalo kumqashi.",
      askQuery: "Yikuphi ukuvikeleka enginakho mayelana nedipozithi nokuqashisa okubhaliwe?",
    },
    "academy-consumer-unfair-terms": {
      title: "Abahlinzeki abakwazi ukuthembela emigomweni engalungile",
      summary: "Izinkontileka zabathengi kufanele zibe nobulungisa, ziqondakale, futhi zingabi nomthwalo ohlangothini olulodwa kuphela.",
      explanation: "Umthetho Wokuvikelwa Kwabathengi ubalulekile ngaphezu kokuthenga ezitolo. Uthinta nezivumelwano zezinsiza, izimali ezifihliwe, imigomo yokuzikhulula, nezinkontileka ezijwayelekile. Uma igama lenkontileka libandlulula kakhulu, lididayo, noma libeka umthwalo ongafanele kumthengi, izigaba 48 no-49 zingasiza ukuliphikisa noma ziphoqe ukuthi lichazwe kahle.",
      askQuery: "Yini eyenza igama lenkontileka yabathengi libe elingalungile ngaphansi kwe-CPA?",
    },
    "academy-consumer-quality-repair": {
      title: "Izimpahla kufanele zisebenze futhi izinsiza zibe sezingeni elifanele",
      summary: "Abathengi bangafuna izimpahla ezisezingeni futhi bangaba nelungelo lokulungiswa, ukushintshwa noma ukubuyiselwa kwemali lapho kufanele khona.",
      explanation: "Izigaba 55 no-56 ze-CPA zinika abathengi indlela ecacile uma impahla inephutha noma ingafanele ukusetshenziswa okujwayelekile. Umthetho usiza lapho umdayisi efuna ukukuphoqa ekulungiseni okungapheli, engazinaki izinkinga zokuqala, noma ephika isiqinisekiso esisemthethweni sekhwalithi.",
      askQuery: "Ngingacela ini uma umkhiqizo unephutha ngaphansi kwe-CPA?",
    },
    "academy-debt-credit-affordability": {
      title: "Isikweletu akufanele sinikwe ngokunganaki",
      summary: "Ababolekisi kufanele bahlole ukukhokheka ngaphambi kokunikeza isikweletu, futhi isikweletu esinikezwe budedengu singaphikiswa.",
      explanation: "Umthetho Wesikweletu Sikazwelonke ulindele ukuthi ababolekisi bahlole ukuthi uyakwazi yini ukukhokha ngaphambi kokuvuma isikweletu. Uma lokhu kungenziwanga, noma umbolekisi engazinaki izimpawu ezicacile zokucindezeleka kwezezimali, isivumelwano singaphonswa inselelo njengezikweletu ezinikezwe budedengu. Lokho kungathinta ukuphoqelelwa, izimali nezinyathelo zokukhokha.",
      askQuery: "Siyini isikweletu esinikezwe budedengu futhi ngingasiphikisa kanjani ngaphansi kwe-NCA?",
    },
    "academy-debt-credit-enforcement": {
      title: "Ukuphoqelelwa kwezikweletu kunezinyathelo nemingcele",
      summary: "Ababolekisi kufanele balandele izaziso ezifanele nemithetho yezimali ngaphambi kokuphoqelela izivumelwano eziningi.",
      explanation: "I-NCA ayivumeli ukuthi zonke izinsongo zokuthathwa kwempahla noma amasamanisi ziqale ngokushesha. Kukhona izimfuneko zezaziso, imingcele kulokho okungakhokhiswa, kanye nemikhawulo ekuqongeleleni ezinye izimali. Izaziso zesigaba 129 nesigaba 103(5) zivame ukuba semqoka lapho umthengi edinga isikhathi sokuphendula noma efuna ukuhlola isamba esifunwayo.",
      askQuery: "Yiziphi izinyathelo umbolekisi okufanele azilandele ngaphambi kokuphoqelela isikweletu ngaphansi kwe-NCA?",
    },
    "academy-privacy-lawful-collection": {
      title: "Ulwazi lwakho lomuntu siqu alunakuthathwa ngaphandle kwesizathu esisemthethweni",
      summary: "Izinhlangano zidinga isizathu esisemthethweni sokuqoqa nokusebenzisa ulwazi lomuntu siqu, futhi kufanele zikutshele ukuthi zenzani.",
      explanation: "I-POPIA ibalulekile noma nini lapho umqashi, isikole, umnikazi wendawo, umbolekisi noma uhlelo lokusebenza lufuna ulwazi lwakho. Umthetho ufuna isisekelo esisemthethweni sokulucubungula kanye nesaziso esicacile sokuthi ubani oluqoqayo, kungani ludingeka, nokuthi kuzokwenzekani uma ungaluniki. Ukuqoqwa kolwazi okufihliwe noma okweqile yilokho kanye umthetho ofuna ukukunciphisa.",
      askQuery: "Kunini lapho umuntu noma inhlangano ingaqoqa futhi isebenzise ulwazi lwami ngokusemthethweni ngaphansi kwe-POPIA?",
    },
    "academy-privacy-access-correction": {
      title: "Ungacela ukubona nokulungisa imininingwane yakho",
      summary: "I-POPIA inika abantu amalungelo okufinyelela, ukulungisa, futhi kwezinye izikhathi ukuphikisa indlela ulwazi lwabo olusetshenziswa ngayo.",
      explanation: "Uma inkampani, isikole noma umqashi ephethe ulwazi olungalungile noma olungasasebenzi ngawe, i-POPIA ikunikeza amalungelo asebenzayo okubuza ukuthi banani futhi ucele ukulungiswa noma ukusulwa lapho kufanelekile. Lawa malungelo abalulekile lapho amarekhodi angalungile eqala ukuthinta umsebenzi, isikweletu noma ezinye izinsiza.",
      askQuery: "Ngingawasebenzisa kanjani amalungelo ami e-POPIA ukuze ngibone noma ngilungise ulwazi olungami?",
    },
    "academy-safety-protection-order": {
      title: "Ukuhlukumeza kungamiswa ngomyalelo wokuvikela",
      summary: "Umthetho unikeza indlela yenkantolo yokufuna ukuvikeleka ekulandeleni, ezinsongweni nasekuziphatheni okuphindaphindwayo okungafuneki.",
      explanation: "Ukuhlukumeza akukhawulelwe endaweni eyodwa. Kungenzeka ekhaya, emsebenzini, ku-inthanethi, noma ngabantu abasebenzela omunye. Umthetho Wokuvikelwa Ekuhlukumezeni uvumela umuntu ukuthi afake isicelo somyalelo wokuvikela lapho ukuziphatha kudala umonakalo noma kwenza umuntu akholelwe ukuthi umonakalo uzolandela. Lokho kuwusizo olusebenzayo lokuphepha, hhayi nje udaba lobugebengu.",
      askQuery: "Imiyalelo yokuvikela isebenza kanjani ngaphansi koMthetho Wokuvikelwa Ekuhlukumezeni?",
    },
    "academy-safety-interim-order": {
      title: "Izinkantolo zinganikeza ukuvikeleka okuphuthumayo okwesikhashana",
      summary: "Uma amaqiniso ekusekela, inkantolo ingakhipha ukuvikelwa kwesikhashana ngaphambi kokuthi udaba luqedwe ngokuphelele.",
      explanation: "Isivinini sibalulekile ezindabeni zokuhlukumeza. Umthetho uvumela izinkantolo ukuthi zikhiphe imiyalelo yokuvikela yesikhashana lapho kukhona ubufakazi bokuqala futhi uma ubunzima bokulibala budinga ukuvikelwa ngokushesha. Lokho kubalulekile lapho kukhona izinsongo eziqhubekayo, ukulandela, ukuxhumana noma ukwesabisa okwenzeka ngesikhathi kusalindwe ukulalelwa kokugcina.",
      askQuery: "Inkantolo ingawukhipha nini umyalelo wokuvikela wesikhashana wokuhlukumeza?",
    },
  },
  st: {
    "academy-legal-equality-dignity": {
      title: "Tekatekano le seriti di tla pele",
      summary: "Molaotheo o sireletsa tekatekano le seriti mesebetsing, matlong a khiriso, dikolong le ditshebeletsong.",
      explanation: "Dikarolo tsa 9 le 10 tsa Molaotheo di beha motheo wa kamoo batho ba lokelang ho tshwarwa kateng. Ha molao, qeto kapa konteraka e o khetholla ka lebaka la hore na o mang kapa e nyenyefatsa seriti sa hao, seo se ka ba kgahlanong le Molaotheo mme sa tshehetsa tletlebo kapa phephetso lekhotleng.",
      askQuery: "Hlalosa kamoo Molaotheo o sireletsang seriti le tekatekano ya ka bophelong ba letsatsi le letsatsi.",
    },
    "academy-legal-courts-and-fair-process": {
      title: "O ka phephetsa diqeto tse sa lokang",
      summary: "O na le tokelo ya hore qabang ya molao e ahlolwe ka toka ke lekhotla kapa lekala le ikemetseng.",
      explanation: "Karolo ya 34 e bohlokwa hobane e bolela hore ha ho hlokahale hore o amohele feela ho lelekwa, ho tloswa kapa qeto e nngwe e sa lokang. Ho tshwanetse ho be le tshebetso e lokileng, mme tsamaiso ya molao e tshwanetse ho dula e bulehile ha ditokelo tsa hao di ameha.",
      askQuery: "Karolo ya 34 e bolela eng haeba ke batla ho phephetsa qeto e sa lokang?",
    },
    "academy-employment-written-terms": {
      title: "Melao ya mosebetsi wa hao e tshwanetse ho ngolwa",
      summary: "Basebetsi ba tshwanetse ho fumana dintlha tse ngotsweng tse hlalosang moputso, dihora, matsatsi a phomolo le maemo a mang a bohlokwa.",
      explanation: "BCEA e batla hore mohiri a fe mosebetsi dintlha tse ngotsweng tsa mosebetsi le tlhahisoleseding e hlakileng ya moputso. Seo se bohlokwa hobane hangata basebetsi ba hatellwa ka diphetoho kapa dikgaello tse sa hlakang. Dintlha tse ngotsweng di thusa ho bapisa se tshepisitsweng le se etsahalang, mme di ba bopaki bo bohlokwa ha ho na le ngangisano.",
      askQuery: "Ke tlhahisoleseding efe e ngotsweng eo mohiri a lokelang ho mpha yona tlasa BCEA?",
    },
    "academy-employment-dismissal": {
      title: "Ho lelekoa mosebetsing ho tshwanetse ho ba ka toka",
      summary: "Mosebetsi o na le tokelo ya ho se lelekwe ka tsela e sa lokang mme a ka phephetsa ho lelekoa ka ditsela tsa basebetsi.",
      explanation: "Molao wa Dikamano tsa Basebetsi o re mosebetsi o na le tokelo ya ho se lelekwe ka tsela e sa lokang. Toka hangata e hloka lebaka le lokileng le tshebetso e lokileng. Ha o lelekoa hanghang, ntle le hore o utlwahale, kapa ka lebaka le fokolang, o ka kgona ho ya CCMA kapa lekgotleng le tshwanang la basebetsi.",
      askQuery: "Nka tseba jwang hore ho lelekoa ha ka ho ne ho sa loka tlasa LRA?",
    },
    "academy-housing-eviction-court-order": {
      title: "O ke ke wa lelekoa ntle le tshebetso ya lekhotla",
      summary: "Mong wa ntlo kapa lefatshe a ke ke a o tlosa ka molao lapeng ntle le taelo ya lekhotla le tshebetso e lokileng.",
      explanation: "Dikganetsano tsa matlo hangata di qala ka ditshoso, ho notlelwa kantle kapa ho kgaolwa ditshebeletso, empa molao o tiile. PIE le karolo ya 26 ya Molaotheo di hloka tshebetso ya lekhotla pele ho lelekoa. Lekhotla le tshwanetse ho sheba toka le tekatekano, haholoholo ha bana, botsofali, bokooa kapa bolulo ba nako e telele bo ameha.",
      askQuery: "Na mong wa ntlo a ka nteleka kapa a nnotlela kantle ntle le taelo ya lekhotla?",
    },
    "academy-housing-lease-and-deposit": {
      title: "Melao ya khiriso le depositi di tshwanetse ho tshwarwa ka toka",
      summary: "Baqashi ba na le tokelo ya konteraka e ngotsweng ha ba e kopa le ho tshwarwa ka toka ha depositi, tlhahlobo le dikganetsano.",
      explanation: "Molao wa Rental Housing o fana ka tshireletso e sebetsang mabapi le melao ya khiriso, tlhahlobo ya ntlo, dipositi le dikopo tsa mekgwa e sa lokang. O thusa moqashi ha mong wa ntlo a hana ho ngola tumellano, a tshwara depositi ka tsela e sa lokang, kapa a sebedisa maemo a sa hlakang ho jara moqashi kotsi yohle.",
      askQuery: "Ke tshireletso efe eo ke nang le yona mabapi le dipositi le konteraka e ngotsweng ya khiriso?",
    },
    "academy-consumer-unfair-terms": {
      title: "Bafani ba ditshebeletso ba ke ke ba itshetleha ka maemo a sa lokang",
      summary: "Dikonteraka tsa bareki di tshwanetse ho ba ka toka, di utlwahale, mme di se ke tsa jara moreki kotsi ka lehlakore le le leng feela.",
      explanation: "Molao wa Tshireletso ya Bareki o bohlokwa le ka nqane ho ho reka. O ama le ditumellano tsa ditshebeletso, ditefiso tse patilweng, maemo a ho itshireletsa le dikonteraka tse tlwaelehileng. Ha maemo a konteraka a le thata haholo, a ferekanya kapa a sutumelletsa moreki kotsi e sa lokang, dikarolo tsa 48 le 49 di ka thusa ho a phephetsa kapa ho qobella tlhaloso e hlakileng.",
      askQuery: "Ke eng e etsang hore maemo a konteraka ya moreki a be a sa loka tlasa CPA?",
    },
    "academy-consumer-quality-repair": {
      title: "Dihlahisoa di tshwanetse ho sebetsa mme ditshebeletso di be maemong a loketseng",
      summary: "Bareki ba ka batla dihlahiswa tsa boleng mme ba ka ba le tokelo ya tokiso, phetolo kapa puseletso ha ho tshwanetse.",
      explanation: "Dikarolo tsa 55 le 56 tsa CPA di fa bareki tsela e hlakileng ha sehlahisoa se na le phoso kapa se sa tshwanela tshebediso e tlwaelehileng. Molao o thusa ha morekisi a batla ho o qobella tokiso e sa feleng, a iphapanyetsa diphoso tsa pele, kapa a hana waranti ya molao ya boleng.",
      askQuery: "Nka kopa eng haeba sehlahisoa se na le phoso tlasa CPA?",
    },
    "academy-debt-credit-affordability": {
      title: "Mokitlane ha oa lokela ho fuwa ka bohlaswa",
      summary: "Bafani ba mekitlane ba tshwanetse ho lekola bokgoni ba ho lefa pele ba fana ka mokitlane, mme mokitlane o fuwe ka bohlaswa o ka phephetswa.",
      explanation: "Molao wa National Credit o lebeletse hore bafani ba mekitlane ba hlahlobe hore na o ka kgona ho lefa pele ba amohela mokitlane. Ha seo se sa etswe, kapa mofani a iphapanyetsa matshwao a hlakileng a mokoloto o feteletseng, tumellano e ka hlaselwa e le reckless credit. Seo se ka ama qobello, ditefiso le mehato ya ho patala.",
      askQuery: "Reckless credit ke eng mme nka e phephetsa jwang tlasa NCA?",
    },
    "academy-debt-credit-enforcement": {
      title: "Qobello ya mokoloto e na le mehato le meedi",
      summary: "Bafani ba mekitlane ba tshwanetse ho latela ditemoso tse loketseng le melao ya ditefiso pele ba qobella ditumellano tse ngata.",
      explanation: "NCA ha e dumelle hore ditshoso tsohle tsa ho nkwa ha thepa kapa summons di qale hanghang. Ho na le ditlhoko tsa ditemoso, meedi ya se ka lefiswang, le mefokolo ya hore na ditjhelete tse itseng di ka bokellana jwang. Ditemoso tsa karolo ya 129 le karolo ya 103(5) di atisa ho ba tsa bohlokwa ha moreki a hloka nako ya ho araba kapa a batla ho hlahloba tjhelete e tsekang.",
      askQuery: "Ke mehato efe eo mofani wa mokitlane a lokelang ho e latela pele a qobella mokoloto tlasa NCA?",
    },
    "academy-privacy-lawful-collection": {
      title: "Tlhahisoleseding ya hao ya botho e ke ke ya bokellwa ntle le lebaka le molaong",
      summary: "Mekgatlo e hloka lebaka le molaong la ho bokella le ho sebedisa tlhahisoleseding ya botho, mme e tshwanetse ho o tsebisa seo e se etsang.",
      explanation: "POPIA e bohlokwa nako le nako ha mohiri, sekolo, mong wa ntlo, mofani wa mokitlane kapa app e batla tlhahisoleseding ya hao ya botho. Molao o hloka motheo o molaong wa processing le tsebiso e hlakileng ya hore na ke mang ya bokellang tlhahisoleseding, hobaneng e hlokahala, le hore na ho tla etsahalang ha o sa e fe. Pokello e patilweng kapa e feteletseng ke yona ntho eo molao o batlang ho e fokotsa.",
      askQuery: "Ke neng moo motho kapa mokgatlo a ka bokellang le ho sebedisa tlhahisoleseding ya ka ka molao tlasa POPIA?",
    },
    "academy-privacy-access-correction": {
      title: "O ka kopa ho bona le ho lokisa data ya hao",
      summary: "POPIA e fa batho ditokelo tsa ho fihlella, ho lokisa, mme maemong a mang ho hanyetsa tsela eo tlhahisoleseding ya bona e sebediswang ka yona.",
      explanation: "Ha khamphani, sekolo kapa mohiri a boloka tlhahisoleseding e fosahetseng kapa e sa ntjhafatswang ka wena, POPIA e o fa ditokelo tse sebetsang tsa ho botsa hore na ba bolokile eng le ho kopa tokiso kapa ho hlakolwa moo ho loketseng. Ditokelo tsena di bohlokwa ha lirekoto tse mpe di qala ho ama mosebetsi, mokitlane kapa ditshebeletso tse ding.",
      askQuery: "Nka sebedisa ditokelo tsa ka tsa POPIA jwang ho bona kapa ho lokisa tlhahisoleseding e mabapi le nna?",
    },
    "academy-safety-protection-order": {
      title: "Tlhekefetso e ka emiswa ka taelo ya tshireletso",
      summary: "Molao o fana ka tsela ya lekhotla ya ho batla tshireletso kgahlanong le ho latelwa, ditshoso le boitshwaro bo sa batleheng bo pheta-phetoang.",
      explanation: "Tlhekefetso ha e felle sebakeng se le seng. E ka etsahala lapeng, mosebetsing, inthaneteng kapa ka batho ba etsetsang motho e mong. Molao wa Protection from Harassment o dumella motho ho etsa kopo ya taelo ya tshireletso ha boitshwaro bo baka kotsi kapa bo etsa hore motho a dumele ka mabaka a utlwahalang hore kotsi e tla latela. Seo se e etsa sesebediswa sa bohlokwa sa polokeho, eseng taba ya botlokotsebe feela.",
      askQuery: "Litaelo tsa tshireletso di sebetsa jwang tlasa Protection from Harassment Act?",
    },
    "academy-safety-interim-order": {
      title: "Makhotla a ka fana ka tshireletso ya nakwana ka potlako",
      summary: "Ha dintlha di e tshehetsa, lekhotla le ka fana ka tshireletso ya nakwana pele taba e qetwa ka botlalo.",
      explanation: "Lebelo le bohlokwa ditabeng tsa tlhekefetso. Molao o dumella makhotla ho fana ka interim protection order moo ho nang le bopaki ba pele mme boima ba kotsi bo hloka tshireletso hanghang. Seo se bohlokwa ha ho na le ditshoso tse tswelang pele, ho latelwa, puisano e sa batleheng kapa ho tshoswa ha nyewe e sa ntse e emetse ho utluwa.",
      askQuery: "Lekhotla le ka fana neng ka interim protection order ya tlhekefetso?",
    },
  },
  af: {
    "academy-legal-equality-dignity": {
      title: "Gelykheid en waardigheid kom eerste",
      summary: "Die Grondwet beskerm gelyke behandeling en waardigheid in werk, huur, skole en dienste.",
      explanation: "Artikels 9 en 10 van die Grondwet vorm die basis vir hoe mense behandel moet word. As 'n reël, besluit of kontrak jou onbillik tref oor wie jy is of jou waardigheid afbreek, kan dit ongrondwetlik wees en 'n klagte of hofuitdaging ondersteun.",
      askQuery: "Verduidelik hoe die Grondwet my waardigheid en gelykheid in die alledaagse lewe beskerm.",
    },
    "academy-legal-courts-and-fair-process": {
      title: "Jy kan onbillike besluite aanveg",
      summary: "Jy het die reg dat regsdispute billik deur 'n hof of onafhanklike tribunaal beslis word.",
      explanation: "Artikel 34 is prakties belangrik omdat dit beteken jy hoef nie bloot 'n onbillike uitsetting, ontslag, skuldstap of ander besluit te aanvaar nie. Daar moet 'n billike proses wees, en die regstelsel moet vir jou oop bly wanneer jou regte geraak word.",
      askQuery: "Wat beteken artikel 34 as ek 'n onbillike besluit wil aanveg?",
    },
    "academy-employment-written-terms": {
      title: "Jou diensvoorwaardes moet op skrif wees",
      summary: "Werknemers behoort duidelike skriftelike voorwaardes oor loon, ure, verlof en ander kernvoorwaardes te ontvang.",
      explanation: "Die BCEA vereis dat werkgewers skriftelike besonderhede van diens en behoorlike betaalinligting gee. Dit is belangrik omdat werkers dikwels onder druk geplaas word met veranderende terme of onduidelike aftrekkings. Geskrewe voorwaardes help jou om te vergelyk wat belowe is met wat werklik gebeur, en word belangrike bewys in dispute.",
      askQuery: "Watter skriftelike diensinligting moet my werkgewer my volgens die BCEA gee?",
    },
    "academy-employment-dismissal": {
      title: "Ontslag moet billik wees",
      summary: "Werkers het die reg om nie onbillik ontslaan te word nie en kan ontslag deur arbeidsprosesse aanveg.",
      explanation: "Die Arbeidsverhoudinge Wet sê werknemers het die reg om nie onbillik ontslaan te word nie. Billikheid vereis gewoonlik beide 'n billike rede en 'n billike proses. As jy skielik, sonder 'n verhoor, of vir 'n swak rede ontslaan word, kan die CCMA of 'n bedingingsraad vir jou beskikbaar wees.",
      askQuery: "Hoe weet ek of my ontslag volgens die LRA onbillik was?",
    },
    "academy-housing-eviction-court-order": {
      title: "Jy kan nie sonder 'n hofproses uitgesit word nie",
      summary: "'n Verhuurder of eienaar mag jou nie wettig uit 'n huis verwyder sonder 'n hofbevel en 'n regverdige proses nie.",
      explanation: "Behuisingsgeskille eskaleer dikwels deur dreigemente, uitsluitings of die afsluiting van dienste, maar die wet is strenger. PIE en artikel 26 van die Grondwet vereis 'n hofproses voor uitsetting. Die hof moet regverdigheid en billikheid oorweeg, veral waar kinders, ouderdom, gestremdheid of lang besetting betrokke is.",
      askQuery: "Kan 'n verhuurder my uitsit of uitsluit sonder 'n hofbevel?",
    },
    "academy-housing-lease-and-deposit": {
      title: "Huurvoorwaardes en deposito's moet billik hanteer word",
      summary: "Huurders het die reg op 'n skriftelike huurkontrak op versoek en billike hantering van deposito's, inspeksies en geskille.",
      explanation: "Die Rental Housing Act gee praktiese beskerming rondom huurvoorwaardes, inspeksies, deposito's en klagtes oor onbillike praktyke. Dit help huurders om terug te druk wanneer 'n verhuurder weier om die ooreenkoms te dokumenteer, 'n deposito onbillik terughou, of vae huurtaal gebruik om al die risiko op die huurder te plaas.",
      askQuery: "Watter beskerming het ek rondom huurdeposito's en skriftelike huurkontrakte?",
    },
    "academy-consumer-unfair-terms": {
      title: "Verskaffers kan nie op onbillike kontrakbepalings steun nie",
      summary: "Verbruikerskontrakte moet billik, verstaanbaar en nie eenzijdig in hul risikoverdeling wees nie.",
      explanation: "Die Wet op Verbruikersbeskerming is belangrik ver buite gewone inkopies. Dit raak ook dienskontrakte, versteekte fooie, vrywaringe en standaardvorm-kontrakte. As 'n bepaling buitensporig eenkantig, verwarrend of onredelik riskant vir die verbruiker is, kan artikels 48 en 49 help om dit aan te veg of beter openbaarmaking te vereis.",
      askQuery: "Wat maak 'n verbruikerskontrakbepaling onbillik onder die CPA?",
    },
    "academy-consumer-quality-repair": {
      title: "Goedere moet werk en dienste moet 'n redelike standaard haal",
      summary: "Verbruikers kan kwaliteit goedere eis en kan in die regte geval herstel, vervanging of terugbetaling kry.",
      explanation: "Artikels 55 en 56 van die CPA gee verbruikers 'n praktiese roete wanneer produkte foutief is of nie geskik is vir hul gewone doel nie. Die wet help wanneer 'n verkoper jou in eindelose herstelwerk wil vasdruk, vroeë gebreke ignoreer, of die geïmpliseerde waarborg van gehalte ontken.",
      askQuery: "Wat kan ek vra as 'n produk foutief is onder die CPA?",
    },
    "academy-debt-credit-affordability": {
      title: "Krediet behoort nie roekeloos toegestaan te word nie",
      summary: "Uitleners moet bekostigbaarheid toets voordat hulle krediet toestaan, en roekelose krediet kan aangeveg word.",
      explanation: "Die National Credit Act verwag dat kredietverskaffers moet toets of jy die krediet kan bekostig voordat dit goedgekeur word. As dit nie gebeur nie, of die uitlener duidelike tekens van oorverskuldiging ignoreer, kan die ooreenkoms as roekelose krediet aangeval word. Dit kan handhawing, fooie en terugbetalingsstappe raak.",
      askQuery: "Wat is roekelose krediet en hoe kan ek dit onder die NCA aanveg?",
    },
    "academy-debt-credit-enforcement": {
      title: "Skuldinvordering het stappe en perke",
      summary: "Kredietverskaffers moet behoorlike kennisgewings en fooiereëls volg voordat baie ooreenkomste afgedwing word.",
      explanation: "Die NCA laat nie toe dat elke dreigement van terugneming of dagvaarding onmiddellik begin nie. Daar is kennisgewingsvereistes, perke op wat gehef mag word, en beperkings op die opbou van sekere bedrae. Artikel 129-kennisgewings en artikel 103(5) is dikwels belangrik wanneer 'n verbruiker tyd nodig het om te reageer of die bedrag wat geëis word wil toets.",
      askQuery: "Watter stappe moet 'n kredietverskaffer volg voordat 'n skuld onder die NCA afgedwing word?",
    },
    "academy-privacy-lawful-collection": {
      title: "Jou persoonlike inligting kan nie sonder 'n behoorlike grondslag versamel word nie",
      summary: "Organisasies het 'n wettige rede nodig om persoonlike inligting te versamel en te gebruik, en hulle moet jou vertel wat hulle doen.",
      explanation: "POPIA is belangrik wanneer 'n werkgewer, skool, verhuurder, kredietverskaffer of toepassing jou persoonlike inligting vra. Die wet vereis 'n wettige grondslag vir verwerking en duidelike kennis oor wie die data insamel, waarom dit nodig is, en wat gebeur as jy dit nie voorsien nie. Versteekte of buitensporige dataversameling is presies wat die wet probeer beperk.",
      askQuery: "Wanneer mag iemand my persoonlike inligting wettiglik versamel en gebruik onder POPIA?",
    },
    "academy-privacy-access-correction": {
      title: "Jy kan vra om jou data te sien en reg te stel",
      summary: "POPIA gee mense regte om toegang te kry, regstelling te vra, en soms beswaar te maak teen hoe hul inligting gebruik word.",
      explanation: "As 'n maatskappy, skool of werkgewer verkeerde of verouderde data oor jou hou, gee POPIA jou praktiese regte om te vra wat hulle hou en om regstelling of skrapping te versoek waar dit gepas is. Hierdie regte raak belangrik wanneer swak rekords begin inmeng met werk, krediet, dienste of jou reputasie.",
      askQuery: "Hoe gebruik ek my POPIA-regte om inligting oor my te sien of reg te stel?",
    },
    "academy-safety-protection-order": {
      title: "Teistering kan met 'n beskermingsbevel gestop word",
      summary: "Die wet gee 'n hofroete om beskerming te vra teen stalk, dreigemente en herhaalde ongewenste gedrag.",
      explanation: "Teistering is nie beperk tot een plek nie. Dit kan by die huis, by die werk, aanlyn, of deur mense namens iemand anders gebeur. Die Protection from Harassment Act laat 'n persoon toe om vir 'n beskermingsbevel aansoek te doen wanneer gedrag skade veroorsaak of 'n redelike vrees vir skade skep. Dit maak dit 'n praktiese veiligheidsinstrument, nie net 'n strafregtelike saak nie.",
      askQuery: "Hoe werk beskermingsbevele onder die Protection from Harassment Act?",
    },
    "academy-safety-interim-order": {
      title: "Howe kan dringende tussentydse beskerming gee",
      summary: "Waar die feite dit regverdig, kan 'n hof tussentydse beskerming gee voordat die saak finaal beslis word.",
      explanation: "Spoed is belangrik in teisteringsake. Die wet laat howe toe om tussentydse beskermingsbevele uit te reik waar daar prima facie bewyse is en waar die balans van nadeel onmiddellike beskerming regverdig. Dit is belangrik waar daar voortgesette dreigemente, nasporing, kontak of intimidasie is terwyl 'n finale verhoor nog hangende is.",
      askQuery: "Wanneer kan 'n hof 'n tussentydse beskermingsbevel vir teistering uitreik?",
    },
  },
};

export function localizeRightsAcademyTracks(tracks: RightsAcademyTrack[], locale: string): RightsAcademyTrack[] {
  if (locale === "en") {
    return tracks;
  }

  const overrides = RIGHTS_ACADEMY_LOCALE_OVERRIDES[locale as Exclude<SupportedRightsLocale, "en">];
  if (!overrides) {
    return tracks;
  }

  return tracks.map((track) => ({
    ...track,
    lessons: track.lessons.map((lesson) => {
      const lessonOverride = overrides[lesson.id] ?? {};
      const localizedLawTitle = localizeRightsActName(lesson.lawTitle, locale);

      return {
        ...lesson,
        ...lessonOverride,
        lawTitle: lessonOverride.lawTitle ?? localizedLawTitle,
        sourceQuote: lessonOverride.sourceQuote ?? localizeRightsQuote(lesson.sourceQuote ?? "", locale),
        primaryCitation:
          lessonOverride.primaryCitation ??
          localizeRightsPrimaryCitation(lesson.primaryCitation, lesson.lawTitle, locale),
        citations: lesson.citations.map((citation) => ({
          ...citation,
          actName: localizeRightsActName(citation.actName, locale),
          excerpt: localizeRightsCitationExcerpt(citation.excerpt, locale),
        })),
      };
    }),
  }));
}

function localizeRightsActName(actName: string, locale: string): string {
  const normalizedActName = actName.trim();

  if (locale === "zu") {
    switch (normalizedActName) {
      case "Constitution of the Republic of South Africa":
        return "UMthethosisekelo weRiphabhulikhi yaseNingizimu Afrika";
      case "Basic Conditions of Employment Act":
        return "Umthetho Wezimo Eziyisisekelo Zokuqashwa";
      case "Labour Relations Act":
        return "Umthetho Wezobudlelwano Emsebenzini";
      case "Prevention of Illegal Eviction from and Unlawful Occupation of Land Act":
        return "Umthetho Wokuvimbela Ukuxoshwa Okungekho Emthethweni Nokuhlala Emhlabeni Ngokungemthetho";
      case "Rental Housing Act":
        return "Umthetho Wezindlu Zokuqasha";
      case "Consumer Protection Act":
        return "Umthetho Wokuvikelwa Kwabathengi";
      case "National Credit Act":
        return "Umthetho Wesikweletu Sikazwelonke";
      case "Protection of Personal Information Act":
        return "Umthetho Wokuvikelwa Kolwazi Lomuntu Siqu";
      case "Protection from Harassment Act":
        return "Umthetho Wokuvikelwa Ekuhlukunyezweni";
      default:
        return normalizedActName;
    }
  }

  if (locale === "st") {
    switch (normalizedActName) {
      case "Constitution of the Republic of South Africa":
        return "Molaotheo wa Rephaboliki ya Afrika Borwa";
      case "Basic Conditions of Employment Act":
        return "Molao wa Maemo a Motheo a Mosebetsi";
      case "Labour Relations Act":
        return "Molao wa Dikamano tsa Mosebetsi";
      case "Prevention of Illegal Eviction from and Unlawful Occupation of Land Act":
        return "Molao wa Thibelo ya Ho Lelekoa ka Tsela e Seng Molaong le Bolulo bo Seng Molaong";
      case "Rental Housing Act":
        return "Molao wa Matlo a Khiriso";
      case "Consumer Protection Act":
        return "Molao wa Tshireletso ya Bareki";
      case "National Credit Act":
        return "Molao wa Naha wa Mokitlane";
      case "Protection of Personal Information Act":
        return "Molao wa Tshireletso ya Tlhahisoleseding ya Botho";
      case "Protection from Harassment Act":
        return "Molao wa Tshireletso Kgahlanong le Tlhekefetso";
      default:
        return normalizedActName;
    }
  }

  if (locale === "af") {
    switch (normalizedActName) {
      case "Constitution of the Republic of South Africa":
        return "Grondwet van die Republiek van Suid-Afrika";
      case "Basic Conditions of Employment Act":
        return "Wet op Basiese Diensvoorwaardes";
      case "Labour Relations Act":
        return "Wet op Arbeidsverhoudinge";
      case "Prevention of Illegal Eviction from and Unlawful Occupation of Land Act":
        return "Wet op die Voorkoming van Onwettige Uitsetting en Onregmatige Okkupasie van Grond";
      case "Rental Housing Act":
        return "Wet op Huurbehuising";
      case "Consumer Protection Act":
        return "Wet op Verbruikersbeskerming";
      case "National Credit Act":
        return "Nasionale Kredietwet";
      case "Protection of Personal Information Act":
        return "Wet op die Beskerming van Persoonlike Inligting";
      case "Protection from Harassment Act":
        return "Wet op Beskerming teen Teistering";
      default:
        return normalizedActName;
    }
  }

  return normalizedActName;
}

function localizeRightsPrimaryCitation(primaryCitation: string, lawTitle: string, locale: string): string {
  if (!primaryCitation) {
    return primaryCitation;
  }

  const localizedActName = localizeRightsActName(lawTitle, locale);
  if (lawTitle && primaryCitation.startsWith(lawTitle)) {
    return `${localizedActName}${primaryCitation.slice(lawTitle.length)}`;
  }

  return primaryCitation;
}

function localizeRightsQuote(sourceQuote: string, locale: string): string {
  const normalizedQuote = sourceQuote.trim();
  if (!normalizedQuote) {
    return sourceQuote;
  }

  if (locale === "zu") {
    switch (normalizedQuote) {
      case "Everyone is equal before the law and has the right to equal protection and benefit of the law.":
        return "Wonke umuntu uyalingana phambi komthetho futhi unelungelo lokuvikelwa nokuhlomula ngokulinganayo emthethweni.";
      case "Everyone has the right to have any dispute that can be resolved by the application of law decided in a fair public hearing before a court.":
        return "Wonke umuntu unelungelo lokuthi noma iyiphi impikiswano engaxazululwa ngokusetshenziswa komthetho ilalelwe ngokulingana emphakathini phambi kwenkantolo.";
      case "An employer must supply an employee, when the employee commences employment, with the following particulars in writing.":
        return "Umqashi kufanele anike umsebenzi, lapho eqala umsebenzi, imininingwane elandelayo ngokubhala.";
      case "Every employee has the right not to be unfairly dismissed.":
        return "Wonke umsebenzi unelungelo lokungaxoshwa ngokungafanele.";
      case "No one may be evicted from their home, or have their home demolished, without an order of court made after considering all the relevant circumstances.":
        return "Akekho ongaxoshwa ekhaya lakhe, noma indlu yakhe idilizwe, ngaphandle komyalelo wenkantolo owenziwe ngemva kokucubungula zonke izimo ezifanele.";
      case "A lease between a tenant and a landlord, subject to subsection (6), need not be in writing or be subject to a standard format.":
        return "Isivumelwano sokuqasha phakathi komqashi nomnikazi wendlu, ngaphansi kwesigatshana (6), asidingi ukuba sibhalwe phansi noma silandele ifomethi ejwayelekile.";
      case "A supplier must not offer to supply, supply, or enter into an agreement to supply, any goods or services on terms that are unfair, unreasonable or unjust.":
        return "Umhlinzeki akumele anikeze, ahlinzeke, noma angene esivumelwaneni sokuhlinzeka nganoma yiziphi izimpahla noma izinsiza ngemigomo engalungile, engenangqondo noma engenabulungisa.";
      case "Every consumer has a right to receive goods that are reasonably suitable for the purposes for which they are generally intended.":
        return "Wonke umthengi unelungelo lokuthola izimpahla ezilungele ngokwanele injongo ezivame ukusetshenziselwa yona.";
      case "A credit provider must not enter into a credit agreement without first taking reasonable steps to assess the proposed consumer's general understanding and debt repayment history.":
        return "Umhlinzeki wekredithi akumele angene esivumelwaneni sekredithi ngaphandle kokuthatha izinyathelo ezifanele zokuhlola ukuqonda komthengi okuvamile kanye nomlando wokukhokha izikweletu.";
      case "A credit provider may not commence any legal proceedings to enforce the agreement before first providing notice to the consumer.":
        return "Umhlinzeki wekredithi akavunyelwe ukuqala noma yiziphi izinyathelo zomthetho zokuphoqelela isivumelwano ngaphambi kokuba aqale anikeze umthengi isaziso.";
      case "Personal information may only be processed if the processing complies with the conditions for the lawful processing of personal information.":
        return "Ulwazi lomuntu siqu lungacutshungulwa kuphela uma lokho kuhambisana nemibandela yokucutshungulwa ngokusemthethweni kolwazi lomuntu siqu.";
      case "A data subject, having provided adequate proof of identity, has the right to request whether a responsible party holds personal information about the data subject.":
        return "Umuntu othintekayo, uma eseveze ubufakazi obanele bokuthi ungubani, unelungelo lokucela ukwazi ukuthi iqembu elinomthwalo wemfanelo liphethe yini ulwazi lomuntu siqu ngaye.";
      case "A complainant may apply to the court for a protection order if any person is engaging or has engaged in harassment.":
        return "Umfakisikhalo angafaka isicelo enkantolo somyalelo wokuvikela uma noma yimuphi umuntu ehlanganyela noma eke wahlanganyela ekuhlukunyezweni.";
      case "The court must issue an interim protection order if it is satisfied that there is prima facie evidence that the respondent is engaging or has engaged in harassment.":
        return "Inkantolo kufanele ikhiphe umyalelo wokuvikela wesikhashana uma yanelisekile ukuthi kukhona ubufakazi bokuqala bokuthi ophendulayo uhlanganyela noma uke wahlanganyela ekuhlukunyezweni.";
      default:
        return sourceQuote;
    }
  }

  if (locale === "st") {
    switch (normalizedQuote) {
      case "Everyone is equal before the law and has the right to equal protection and benefit of the law.":
        return "Motho e mong le e mong o a lekana pela molao mme o na le tokelo ya tshireletso le molemo o lekanang wa molao.";
      case "Everyone has the right to have any dispute that can be resolved by the application of law decided in a fair public hearing before a court.":
        return "Motho e mong le e mong o na le tokelo ya hore qabang efe kapa efe e ka rarollwang ka tshebediso ya molao e ahlolwe ka toka nyeoeng ya setjhaba pela lekhotla.";
      case "An employer must supply an employee, when the employee commences employment, with the following particulars in writing.":
        return "Mohiri o tshwanetse ho fa mosebetsi, ha mosebetsi a qala mosebetsi, dintlha tse latelang ka mongolo.";
      case "Every employee has the right not to be unfairly dismissed.":
        return "Mosebetsi e mong le e mong o na le tokelo ya ho se lelekwe ka tsela e sa lokang.";
      case "No one may be evicted from their home, or have their home demolished, without an order of court made after considering all the relevant circumstances.":
        return "Ha ho motho ya ka lelekwang lapeng la hae, kapa ntlo ya hae ya heletswa, ntle le taelo ya lekhotla e entsweng kamora ho ela hloko maemo ohle a amehang.";
      case "A lease between a tenant and a landlord, subject to subsection (6), need not be in writing or be subject to a standard format.":
        return "Tumellano ya khiriso pakeng tsa mohiri le mong wa ntlo, tlasa karolwana ya (6), ha e hloke ho ba ka mongolo kapa ho latela sebopeho se tlwaelehileng.";
      case "A supplier must not offer to supply, supply, or enter into an agreement to supply, any goods or services on terms that are unfair, unreasonable or unjust.":
        return "Mofani ha a a tshwanela ho fana, ho fana ka thepa kapa ditshebeletso, kapa ho kena tumellanong ya ho fana ka tsona ka maemo a sa lokang, a sa utloahaleng kapa a se nang toka.";
      case "Every consumer has a right to receive goods that are reasonably suitable for the purposes for which they are generally intended.":
        return "Moreki e mong le e mong o na le tokelo ya ho amohela thepa e loketseng ka mokgwa o utloahalang merero eo ka kakaretso e reretsweng yona.";
      case "A credit provider must not enter into a credit agreement without first taking reasonable steps to assess the proposed consumer's general understanding and debt repayment history.":
        return "Mofani wa mokitlane ha a a tshwanela ho kena tumellanong ya mokitlane ntle le ho qala ka ho nka mehato e utloahalang ya ho lekola kutlwisiso ya moreki le nalane ya hae ya ho patala mekoloto.";
      case "A credit provider may not commence any legal proceedings to enforce the agreement before first providing notice to the consumer.":
        return "Mofani wa mokitlane a ke ke a qala ditsamaiso tsa molao ho qobella tumellano pele a qala ka ho fa moreki tsebiso.";
      case "Personal information may only be processed if the processing complies with the conditions for the lawful processing of personal information.":
        return "Tlhahisoleseding ya botho e ka sebetsetswa feela haeba tshebetso eo e latela dipehelo tsa tshebetso e molaong ya tlhahisoleseding ya botho.";
      case "A data subject, having provided adequate proof of identity, has the right to request whether a responsible party holds personal information about the data subject.":
        return "Motho eo data e amanang le yena, ha a se a fane ka bopaki bo lekaneng ba boitsebiso, o na le tokelo ya ho kopa ho tseba hore na mokga o ikarabellang o tshwere tlhahisoleseding ya botho ka yena.";
      case "A complainant may apply to the court for a protection order if any person is engaging or has engaged in harassment.":
        return "Moqosi a ka etsa kopo lekhotleng bakeng sa taelo ya tshireletso haeba motho ofe kapa ofe a etsa kapa a kile a etsa tlhekefetso.";
      case "The court must issue an interim protection order if it is satisfied that there is prima facie evidence that the respondent is engaging or has engaged in harassment.":
        return "Lekhotla le tshwanetse ho fana ka taelo ya tshireletso ya nakwana haeba le kgodisehile hore ho na le bopaki ba pele ba hore moqosuoa o etsa kapa o kile a etsa tlhekefetso.";
      default:
        return sourceQuote;
    }
  }

  if (locale === "af") {
    switch (normalizedQuote) {
      case "Everyone is equal before the law and has the right to equal protection and benefit of the law.":
        return "Almal is gelyk voor die reg en het die reg op gelyke beskerming en voordeel van die reg.";
      case "Everyone has the right to have any dispute that can be resolved by the application of law decided in a fair public hearing before a court.":
        return "Elkeen het die reg dat enige dispuut wat deur die toepassing van die reg opgelos kan word, in 'n billike openbare verhoor voor 'n hof beslis word.";
      case "An employer must supply an employee, when the employee commences employment, with the following particulars in writing.":
        return "’n Werkgewer moet, wanneer ’n werknemer diens aanvaar, die volgende besonderhede skriftelik aan die werknemer verskaf.";
      case "Every employee has the right not to be unfairly dismissed.":
        return "Elke werknemer het die reg om nie onbillik ontslaan te word nie.";
      case "No one may be evicted from their home, or have their home demolished, without an order of court made after considering all the relevant circumstances.":
        return "Niemand mag uit hul huis gesit word, of hul huis laat sloop word, sonder ’n hofbevel wat gemaak is nadat alle tersaaklike omstandighede oorweeg is nie.";
      case "A lease between a tenant and a landlord, subject to subsection (6), need not be in writing or be subject to a standard format.":
        return "’n Huurooreenkoms tussen ’n huurder en ’n verhuurder hoef, behoudens subartikel (6), nie skriftelik te wees of aan ’n standaardformaat te voldoen nie.";
      case "A supplier must not offer to supply, supply, or enter into an agreement to supply, any goods or services on terms that are unfair, unreasonable or unjust.":
        return "’n Verskaffer mag nie enige goedere of dienste aanbied, verskaf, of ’n ooreenkoms aangaan om dit te verskaf, op bepalings wat onbillik, onredelik of onregverdig is nie.";
      case "Every consumer has a right to receive goods that are reasonably suitable for the purposes for which they are generally intended.":
        return "Elke verbruiker het die reg om goedere te ontvang wat redelikerwys geskik is vir die doeleindes waarvoor dit oor die algemeen bestem is.";
      case "A credit provider must not enter into a credit agreement without first taking reasonable steps to assess the proposed consumer's general understanding and debt repayment history.":
        return "’n Kredietverskaffer mag nie ’n kredietooreenkoms aangaan sonder om eers redelike stappe te neem om die voorgestelde verbruiker se algemene begrip en geskiedenis van skuldbetaling te beoordeel nie.";
      case "A credit provider may not commence any legal proceedings to enforce the agreement before first providing notice to the consumer.":
        return "’n Kredietverskaffer mag geen regstappe begin om die ooreenkoms af te dwing voordat kennis eers aan die verbruiker gegee is nie.";
      case "Personal information may only be processed if the processing complies with the conditions for the lawful processing of personal information.":
        return "Persoonlike inligting mag slegs verwerk word indien die verwerking voldoen aan die voorwaardes vir die regmatige verwerking van persoonlike inligting.";
      case "A data subject, having provided adequate proof of identity, has the right to request whether a responsible party holds personal information about the data subject.":
        return "’n Datasubjek, nadat voldoende bewys van identiteit gelewer is, het die reg om te vra of ’n verantwoordelike party persoonlike inligting oor die datasubjek hou.";
      case "A complainant may apply to the court for a protection order if any person is engaging or has engaged in harassment.":
        return "’n Klaer kan by die hof aansoek doen vir ’n beskermingsbevel indien enige persoon by teistering betrokke is of was.";
      case "The court must issue an interim protection order if it is satisfied that there is prima facie evidence that the respondent is engaging or has engaged in harassment.":
        return "Die hof moet ’n tussentydse beskermingsbevel uitreik indien dit tevrede is dat daar prima facie-bewyse is dat die respondent by teistering betrokke is of was.";
      default:
        return sourceQuote;
    }
  }

  return sourceQuote;
}

function localizeRightsCitationExcerpt(excerpt: string, locale: string): string {
  const normalizedExcerpt = excerpt.trim();
  if (!normalizedExcerpt) {
    return excerpt;
  }

  if (locale === "zu") {
    switch (normalizedExcerpt) {
      case "The equality clause and dignity clause are the starting point whenever treatment is unfair or humiliating.":
        return "Isigaba sokulingana nesigaba sesithunzi kuyisiqalo noma nini lapho ukuphathwa kungalungile noma kwehlisa isithunzi.";
      case "Section 34 protects access to a fair hearing before a court or another independent tribunal.":
        return "Isigaba 34 sivikela ukufinyelela ekulalelweni ngokulingana phambi kwenkantolo noma esinye isigungu esizimele.";
      case "The Act requires written particulars of employment and proper information about remuneration and deductions.":
        return "Umthetho ufuna imininingwane ebhaliwe yomsebenzi kanye nolwazi olufanele ngomholo nezikweletu.";
      case "The LRA protects workers from unfair dismissal and sets out the dispute route for challenging it.":
        return "I-LRA ivikela abasebenzi ekuxoshweni okungafanele futhi ibeka indlela yokuphikisa lokho.";
      case "Evictions from a home require a court order after all relevant circumstances are considered.":
        return "Ukuxoshwa ekhaya kudinga umyalelo wenkantolo ngemva kokuba zonke izimo ezifanele zicutshunguliwe.";
      case "PIE sets the court-driven eviction process and notice requirements.":
        return "I-PIE ibeka inqubo yokuxoshwa eqhutshwa yinkantolo kanye nezimfuneko zesaziso.";
      case "Section 5 covers lease content, joint inspections, deposits, receipts, and related housing protections.":
        return "Isigaba 5 sihlanganisa okuqukethwe yisivumelwano sokuqasha, ukuhlolwa okuhlangene, amadiphozithi, amarisidi, nokunye ukuvikelwa kwezindlu.";
      case "The CPA limits unfair, unreasonable, or unjust terms and requires certain risky clauses to be drawn to the consumer's attention.":
        return "I-CPA ivimbela imigomo engalungile, engenangqondo noma engenabulungisa futhi ifuna ukuthi imigomo ethile enobungozi ivezwe kumthengi.";
      case "The CPA gives consumers rights to safe, good-quality goods and creates an implied warranty of quality.":
        return "I-CPA inika abathengi amalungelo ezimpahla eziphephile nezisezingeni futhi idala isiqinisekiso sekhwalithi esicatshangiwe.";
      case "The NCA links reckless credit to failed affordability checks and over-indebted lending.":
        return "I-NCA ixhumanisa isikweletu esinikezwe budedengu nokuhlolwa okwehlulekile kokukhokheka kanye nokubolekisa okweqile.";
      case "The NCA regulates what can be charged and requires notice before many enforcement steps start.":
        return "I-NCA ilawula okungakhokhiswa futhi ifuna isaziso ngaphambi kokuthi kuqale izinyathelo eziningi zokuphoqelela.";
      case "POPIA requires a lawful basis for processing and obliges responsible parties to notify people about the collection.":
        return "I-POPIA ifuna isisekelo esisemthethweni sokucubungula futhi ibophe izigungu ezinomthwalo wemfanelo ukuthi zazise abantu ngokuqoqwa kolwazi.";
      case "POPIA allows data subjects to request access to personal information and correction or deletion in the proper circumstances.":
        return "I-POPIA ivumela abantu ukuthi bacele ukufinyelela olwazini lomuntu siqu kanye nokulungiswa noma ukusulwa lapho kufanelekile.";
      case "The Act creates the process for obtaining a protection order against harassment.":
        return "Umthetho udala inqubo yokuthola umyalelo wokuvikela ekuhlukunyezweni.";
      case "Interim protection is available when the court sees prima facie harassment and urgency justifies it.":
        return "Ukuvikelwa kwesikhashana kuyatholakala lapho inkantolo ibona ubufakazi bokuqala bokuhlukunyezwa futhi ukuphuthuma kukuvumela.";
      default:
        return excerpt;
    }
  }

  if (locale === "st") {
    switch (normalizedExcerpt) {
      case "The equality clause and dignity clause are the starting point whenever treatment is unfair or humiliating.":
        return "Karolo ya tekatekano le karolo ya seriti ke qalo nako le nako ha tshwaro e sa loka kapa e nyenyefatsa.";
      case "Section 34 protects access to a fair hearing before a court or another independent tribunal.":
        return "Karolo ya 34 e sireletsa phihlello ya nyewe e lokileng pela lekhotla kapa lekgotla le leng le ikemetseng.";
      case "The Act requires written particulars of employment and proper information about remuneration and deductions.":
        return "Molao o hloka dintlha tse ngotsweng tsa mosebetsi le tlhahisoleseding e nepahetseng ka moputso le dikgaello.";
      case "The LRA protects workers from unfair dismissal and sets out the dispute route for challenging it.":
        return "LRA e sireletsa basebetsi ho ho lelekwa ka tsela e sa lokang mme e beha tsela ya ho e phephetsa.";
      case "Evictions from a home require a court order after all relevant circumstances are considered.":
        return "Ho lelekoa lapeng ho hloka taelo ya lekhotla kamora hore maemo ohle a amehang a hlahlojwe.";
      case "PIE sets the court-driven eviction process and notice requirements.":
        return "PIE e beha tshebetso ya ho lelekoa e etelwang pele ke lekhotla le ditlhoko tsa tsebiso.";
      case "Section 5 covers lease content, joint inspections, deposits, receipts, and related housing protections.":
        return "Karolo ya 5 e akaretsa diteng tsa khiriso, tlhahlobo ya mmoho, dipositi, direseiti le tshireletso e amanang le bodulo.";
      case "The CPA limits unfair, unreasonable, or unjust terms and requires certain risky clauses to be drawn to the consumer's attention.":
        return "CPA e fokotsa maemo a sa lokang, a sa utloahaleng kapa a se nang toka mme e hloka hore dikarolo tse itseng tse kotsi di totobatswe ho moreki.";
      case "The CPA gives consumers rights to safe, good-quality goods and creates an implied warranty of quality.":
        return "CPA e fa bareki ditokelo tsa thepa e bolokehileng le ya boleng bo botle mme e theha tiisetso e akantsweng ya boleng.";
      case "The NCA links reckless credit to failed affordability checks and over-indebted lending.":
        return "NCA e hokahanya reckless credit le diteko tse hlolehileng tsa bokgoni ba ho lefa le ho alima batho ba seng ba imetswe ke mekoloto.";
      case "The NCA regulates what can be charged and requires notice before many enforcement steps start.":
        return "NCA e laola se ka lefiswang mme e hloka tsebiso pele mehato e mengata ya qobello e qala.";
      case "POPIA requires a lawful basis for processing and obliges responsible parties to notify people about the collection.":
        return "POPIA e hloka motheo o molaong wa tshebetso mme e tlama mekga e ikarabellang ho tsebisa batho ka pokello ya tlhahisoleseding.";
      case "POPIA allows data subjects to request access to personal information and correction or deletion in the proper circumstances.":
        return "POPIA e dumella batho bao data e amanang le bona ho kopa phihlello ya tlhahisoleseding ya botho le tokiso kapa hlakolo maemong a loketseng.";
      case "The Act creates the process for obtaining a protection order against harassment.":
        return "Molao o theha tshebetso ya ho fumana taelo ya tshireletso kgahlanong le tlhekefetso.";
      case "Interim protection is available when the court sees prima facie harassment and urgency justifies it.":
        return "Tshireletso ya nakwana e fumaneha ha lekhotla le bona bopaki ba pele ba tlhekefetso mme potlako e e lokafatsa.";
      default:
        return excerpt;
    }
  }

  if (locale === "af") {
    switch (normalizedExcerpt) {
      case "The equality clause and dignity clause are the starting point whenever treatment is unfair or humiliating.":
        return "Die gelykheidsklousule en waardigheidsklousule is die beginpunt wanneer behandeling onbillik of vernederend is.";
      case "Section 34 protects access to a fair hearing before a court or another independent tribunal.":
        return "Artikel 34 beskerm toegang tot 'n billike verhoor voor 'n hof of ander onafhanklike tribunaal.";
      case "The Act requires written particulars of employment and proper information about remuneration and deductions.":
        return "Die wet vereis skriftelike diensbesonderhede en behoorlike inligting oor vergoeding en aftrekkings.";
      case "The LRA protects workers from unfair dismissal and sets out the dispute route for challenging it.":
        return "Die WAV beskerm werkers teen onbillike ontslag en bepaal die dispuutroete om dit aan te veg.";
      case "Evictions from a home require a court order after all relevant circumstances are considered.":
        return "Uitsettings uit 'n huis vereis 'n hofbevel nadat alle tersaaklike omstandighede oorweeg is.";
      case "PIE sets the court-driven eviction process and notice requirements.":
        return "PIE bepaal die hofgedrewe uitsettingsproses en kennisgewingvereistes.";
      case "Section 5 covers lease content, joint inspections, deposits, receipts, and related housing protections.":
        return "Artikel 5 dek huurinhoud, gesamentlike inspeksies, deposito's, kwitansies en verwante behuisingsbeskerming.";
      case "The CPA limits unfair, unreasonable, or unjust terms and requires certain risky clauses to be drawn to the consumer's attention.":
        return "Die CPA beperk onbillike, onredelike of onregverdige bepalings en vereis dat sekere riskante klousules onder die verbruiker se aandag gebring word.";
      case "The CPA gives consumers rights to safe, good-quality goods and creates an implied warranty of quality.":
        return "Die CPA gee verbruikers regte op veilige goedere van goeie gehalte en skep 'n geïmpliseerde waarborg van gehalte.";
      case "The NCA links reckless credit to failed affordability checks and over-indebted lending.":
        return "Die NKW verbind roekelose krediet aan mislukte bekostigbaarheidstoetse en oorverskuldigde uitleenpraktyke.";
      case "The NCA regulates what can be charged and requires notice before many enforcement steps start.":
        return "Die NKW reguleer wat gehef mag word en vereis kennis voordat baie afdwingingsstappe begin.";
      case "POPIA requires a lawful basis for processing and obliges responsible parties to notify people about the collection.":
        return "POPIA vereis 'n wettige grondslag vir verwerking en verplig verantwoordelike partye om mense oor die insameling in te lig.";
      case "POPIA allows data subjects to request access to personal information and correction or deletion in the proper circumstances.":
        return "POPIA laat datasubjekte toe om toegang tot persoonlike inligting te vra en regstelling of skrapping in die regte omstandighede te versoek.";
      case "The Act creates the process for obtaining a protection order against harassment.":
        return "Die wet skep die proses om 'n beskermingsbevel teen teistering te verkry.";
      case "Interim protection is available when the court sees prima facie harassment and urgency justifies it.":
        return "Tussentydse beskerming is beskikbaar wanneer die hof prima facie-teistering sien en dringendheid dit regverdig.";
      default:
        return excerpt;
    }
  }

  return excerpt;
}
