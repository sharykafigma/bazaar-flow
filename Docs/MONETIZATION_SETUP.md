# Belt Jam: Conveyor Sort — ads aur purchases ka setup

App ka naam **Belt Jam: Conveyor Sort**, Android package **`com.sharystudio.beltjam`**. LevelPlay, AdMob aur Play Console mein app banate waqt yehi naam aur package likhein.
(Code ke andar folder/namespace abhi bhi `BazaarFlow` hai; player ko yeh nazar nahi aata.)

Code mein ads (LevelPlay) aur purchases (Unity IAP / Google Play Billing) lag chuke hain. Abhi **test IDs** lagi hain.
Asal kamai shuru karne ke liye neeche wale accounts banane hain, aur saari IDs **sirf ek file** mein badalni hain:

`Assets/BazaarFlow/Scripts/Monetization/MonetizationConfig.cs`

## 1. Kaun se accounts banane hain (checklist)

| # | Account | Kya milega | Kharcha | Kab chahiye |
| --- | --- | --- | --- | --- |
| 1 | **Google Play Console** developer account (play.google.com/console) | App listing, IAP products | $25 ek dafa | Sab se pehle |
| 2 | **Payments profile / merchant account** (Play Console > Settings > Payments profile) | IAP ki kamai bank mein | Muft | IAP bechne se pehle |
| 3 | **Unity account + Unity Cloud project** (cloud.unity.com) | Project ID, LevelPlay link | Muft | Ads se pehle |
| 4 | **LevelPlay** (platform.ironsrc.com, Unity account se login) | **App Key** + 3 **Ad Unit IDs** (Rewarded, Interstitial, Banner) | Muft | Ads |
| 5 | Google AdMob, AppLovin, Mintegral (**abhi nahi**, baad mein) | Zyada ad networks = zyada kamai | Muft | Jab game chal pare (neeche section 8) |
| 6 | **Privacy policy URL** (koi bhi free page, maslan Google Sites) | Play Store aur ad networks dono maangte hain | Muft | Release se pehle |

Shuru mein ads sirf **Unity Ads** network se aayenge (LevelPlay ke zariye). Is ke liye alag Unity Ads account nahi chahiye: LevelPlay mein Unity account se login karte hi Unity Ads network on ho jata hai.

Bank ki tafseel aur tax form (Pakistan se W-8BEN) LevelPlay aur Play Console dono mein payment ke liye bharne honge.

## 2. Mujhe (Claude ko) kya bhejna hai

Ad unit IDs lag chuki hain: `rewarded_main` (ack6nftc7fpdwf6t) aur `interstitial_main` (z7sr4ewldylcjla7). Jab tak **App Key** khaali hai, game khud test ads chalati hai. Test ads zabardasti chalane ke liye `UseTestAds = true`.

Baaqi IDs bhej dein, main `MonetizationConfig.cs` mein laga dunga:

- LevelPlay **Android App Key**
- LevelPlay **Rewarded** aur **Interstitial** ad unit IDs (Banner ID sirf agar banner on karna ho)

Password, bank details ya keystore kabhi chat mein na bhejein.

## 3. Google Play Console mein products (bilkul yehi IDs)

Play Console > Monetize with Play > Products > **In-app products** > Create product:
Product ke naam aur description **English** mein rakhein (game worldwide hai). Product IDs ka `bf_` prefix sirf andar ki pehchan hai, isay waise hi rakhein. Maslan "No Ads", "Starter Pack", "Coin Pouch".

| Product ID | Qisam | Qeemat (USD) | Kya milta hai |
| --- | --- | --- | --- |
| `bf_no_ads` | One-time (non-consumable) | 3.99 | Interstitial/banner band. Rewarded rehte hain |
| `bf_starter_pack` | One-time (non-consumable) | 1.99 | No Ads + 1000 coins + har booster x1. Pehli haar ke baad 72 ghante |
| `bf_booster_bundle` | Consumable | 2.99 | Har booster x3 |
| `bf_coins_1` | Consumable | 0.99 | 300 coins |
| `bf_coins_2` | Consumable | 2.99 | 1000 coins |
| `bf_coins_3` | Consumable | 4.99 | 1800 coins |
| `bf_coins_4` | Consumable | 9.99 | 4000 coins |
| `bf_coins_5` | Consumable | 14.99 | 6500 coins |
| `bf_coins_6` | Consumable | 19.99 | 9000 coins |

Note: Play Console products tab tab khulta hai jab ek signed AAB kisi testing track (internal testing) pe upload ho jaye aur `com.android.vending.BILLING` permission ho (Unity IAP khud daalta hai).
Test karne ke liye: Play Console > Settings > **License testing** mein apna Gmail daalein; phir khareed muft test card se hoti hai.

## 4. Unity mein packages (Window > Package Manager)

1. **Ads Mediation** (`com.unity.services.levelplay`). "Ads IAP Mediation Adaptor" ya "Advertisement Legacy" NAHI.
   Install pe "Mobile Dependency Resolver" import karein. Phir **Ads Mediation > Network Manager** mein sirf **Unity Ads** adapter install karein (AdMob nahi).
   Phir phir Assets > Mobile Dependency Resolver > Android Resolver > Force Resolve.
2. **In-App Purchasing** (`com.unity.purchasing`) version **5.x**.
3. Menu **Belt Jam > Monetization > Refresh SDK Defines** dabayein. Console mein `LevelPlay=on, IAP=on` aana chahiye.
   (Yeh khud bhi hota hai. Package ke baghair bhi game compile hoti hai: editor mein ads/khareed "simulate" hote hain.)
4. File > Build Profiles > **Android** > Switch Platform. Editor mein mock ads sirf Android target pe aate hain.
5. Android 13+ ke liye `Assets/Plugins/Android/AndroidManifest.xml` (custom manifest) mein:
   `<uses-permission android:name="com.google.android.gms.permission.AD_ID"/>`

## 5. Ads ke rules (design doc se, `MonetizationConfig.cs` mein badal sakte hain)

- **Rewarded** (player ki marzi): Belt Jam pe Continue, jeet pe Dugne coins, roz ka muft booster (Dukaan mein).
- **Interstitial**: level 8 se pehle nahi; sirf jeet ke baad "Agla level" pe; har 2 jeet mein ek; kam se kam 90 second faasla; lagataar 2 haar ke baad nahi; No Ads walon ko kabhi nahi.
- **Banner**: design doc ke mutabiq band (`BannerEnabled = false`). On karein to sirf jeet/jam screen pe neeche chhota banner.
- Ad ke dauran game ruk jati hai. Sound thread `AdsService.FullscreenAdChanged` pe awaaz band kar sakta hai.

## 6. Release se pehle

- `MonetizationConfig.cs` mein saari TODO IDs asal wali.
- `LaunchLevelPlayTestSuite = true` karke ek dafa phone pe Test Suite chalayein (Development Build), phir wapas `false`.
- GDPR (Europe) ke liye consent screen: LevelPlay `LevelPlayPrivacySettings.SetGDPRConsent(...)` `AdsService.InitSdk()` mein Init se pehle.
- Play Console: **Data safety** form (Advertising ID, purchase history), **Ads** declaration "Haan, ads hain", privacy policy URL.
- Google Play receipt validation (optional, fraud ke khilaf): Services > In-App Purchasing > Receipt Validation Obfuscator, phir `IapService.OnPurchasePending` mein CrossPlatformValidator.

## 7. Code ka naqsha

| File | Kaam |
| --- | --- |
| `Monetization/MonetizationConfig.cs` | Saari IDs, qeematein aur ad rules ek jagah |
| `Monetization/Economy.cs` | Save system: coins, boosters, No Ads, offers (PlayerPrefs `bf_save`, JSON) |
| `Monetization/ShopCatalog.cs` | Products ki list aur har ek kya deta hai; booster naam |
| `Monetization/AdsService.cs` | LevelPlay rewarded / interstitial / banner, frequency caps |
| `Monetization/IapService.cs` | Unity IAP v5: khareed, confirm, restore, local qeemat |
| `Monetization/ShopUI.cs` | Saada Dukaan screen (UI thread ki asal screen isay replace kar sakti hai) |
| `Monetization/Editor/MonetizationDefines.cs` | Package hon to `BF_LEVELPLAY` / `BF_IAP` on karta hai |

Gameplay se jor: `GameManager` mein `ContinueWithAd`, `DoubleCoinsWithAd`, `NextLevel`, `RetryAfterJam`, `UseBooster`.
Boosters abhi chalte hain: **Sweep** (code mein Jhaaru) aur **Reveal** (Chashma). Extra Belt (ExtraPatri) aur Reverse (UltiBelt) gameplay mein ban jayen to `Economy.LiveBoosters` mein daal dein, shop aur packs khud un ko shamil kar lenge.

## 8. Baad mein AdMob (ya aur networks) kaise jorein

Code badalne ki zaroorat nahi. Sirf:
1. AdMob account banayein, app add karein (`com.sharystudio.beltjam`), AdMob App ID lein.
2. LevelPlay dashboard > SDK Networks mein AdMob on karke us ke ad unit IDs daalein.
3. Unity: Ads Mediation > Network Manager > AdMob adapter install. Phir Ads Mediation > Developer Settings > LevelPlay Mediation Settings mein AdMob Android App Key. Force Resolve.
