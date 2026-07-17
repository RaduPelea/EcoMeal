using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Ecomeal.client.Services;

public class LocalizationService
{
    private readonly ProtectedLocalStorage _storage;

    public string Language { get; private set; } = "en";
    public event Action? OnChanged;

    public LocalizationService(ProtectedLocalStorage storage)
    {
        _storage = storage;
    }

    public async Task LoadAsync()
    {
        try
        {
            var result = await _storage.GetAsync<string>("language");
            if (result.Success && !string.IsNullOrEmpty(result.Value))
                Language = result.Value;
        }
        catch
        {
            try { await _storage.DeleteAsync("language"); } catch { }
        }
    }

    public async Task SetLanguageAsync(string language)
    {
        Language = language;
        await _storage.SetAsync("language", language);
        OnChanged?.Invoke();
    }

    public string this[string key] => T(key);

    public string T(string key)
    {
        if (!Texts.TryGetValue(key, out var values))
            return key;

        return Language == "ro" ? values[1] : values[0];
    }

    // [english, romanian]
    private static readonly Dictionary<string, string[]> Texts = new()
    {
        ["nav.home"] = ["Home", "Acasă"],
        ["nav.order"] = ["Order", "Comandă"],
        ["nav.endingSoon"] = ["Ending Soon", "Expiră curând"],
        ["nav.favorites"] = ["Favorites", "Favorite"],
        ["nav.myOrders"] = ["My Orders", "Comenzile mele"],
        ["nav.profile"] = ["Profile", "Profil"],
        ["nav.login"] = ["Login", "Autentificare"],
        ["nav.signUp"] = ["Sign Up", "Înregistrare"],
        ["nav.logout"] = ["Logout", "Deconectare"],
        ["nav.manage"] = ["Manage", "Administrare"],
        ["nav.stats"] = ["Stats", "Statistici"],
        ["nav.users"] = ["Users", "Utilizatori"],
        ["nav.myBusiness"] = ["My Business", "Afacerea mea"],
        ["nav.receivedOrders"] = ["Received Orders", "Comenzi primite"],
        ["nav.salesStats"] = ["Sales", "Vânzări"],
        ["nav.noCity"] = ["No city", "Fără oraș"],

        ["footer.ownRestaurant"] = ["Own a restaurant?", "Ai un restaurant?"],
        ["footer.becomePartner"] = ["Want to become a partner? →", "Vrei să devii partener? →"],
        ["footer.partner"] = ["Partner", "Partener"],

        ["logout.title"] = ["Log out?", "Te deconectezi?"],
        ["logout.text"] = ["Are you sure you want to log out of your account?", "Sigur vrei să te deconectezi de la contul tău?"],
        ["logout.cancel"] = ["Cancel", "Renunță"],
        ["logout.confirm"] = ["Log out", "Deconectare"],

        ["review.howWas"] = ["How was {0}?", "Cum a fost la {0}?"],
        ["review.pickedUpText"] = ["Your order \"{0}\" was picked up. Leave a quick review!", "Comanda ta „{0}” a fost ridicată. Lasă un review rapid!"],
        ["review.customerText"] = ["The order \"{0}\" was picked up. Rate this customer!", "Comanda „{0}” a fost ridicată. Evaluează acest client!"],
        ["review.pickRating"] = ["Pick a rating...", "Alege o notă..."],
        ["review.excellent"] = ["5 - Excellent", "5 - Excelent"],
        ["review.good"] = ["4 - Good", "4 - Bun"],
        ["review.okay"] = ["3 - Okay", "3 - Acceptabil"],
        ["review.poor"] = ["2 - Poor", "2 - Slab"],
        ["review.bad"] = ["1 - Bad", "1 - Rău"],
        ["review.commentPlaceholder"] = ["Comment (optional)", "Comentariu (opțional)"],
        ["review.maybeLater"] = ["Maybe later", "Poate mai târziu"],
        ["review.submitReview"] = ["Submit review", "Trimite review-ul"],
        ["review.submitRating"] = ["Submit rating", "Trimite nota"],

        ["home.heroTitle"] = ["Great food, less waste", "Mâncare bună, mai puțină risipă"],
        ["home.heroSubtitle"] = ["EcoMeal connects you with local restaurants offering delicious meals at a discount before they go to waste. Save money, help the planet.",
            "EcoMeal te conectează cu restaurante locale care oferă mâncare delicioasă la reducere, înainte să ajungă la gunoi. Economisești bani și ajuți planeta."],
        ["home.orderNow"] = ["Order Now", "Comandă acum"],
        ["home.browseCategories"] = ["Browse Categories", "Vezi categoriile"],
        ["home.ctaTitle"] = ["Ready to save on your next meal?", "Gata să economisești la următoarea masă?"],
        ["home.ctaText"] = ["Join EcoMeal and start ordering discounted food today.", "Alătură-te EcoMeal și comandă mâncare cu reducere chiar azi."],

        ["endingSoon.title"] = ["Ending Soon", "Expiră curând"],
        ["endingSoon.subtitle"] = ["Pickup windows closing within the next hour — many at a discount. Grab them before they're gone!",
            "Ferestre de ridicare care se închid în următoarea oră — multe cu reducere. Prinde-le până nu dispar!"],
        ["endingSoon.empty"] = ["Nothing is expiring soon. Check back later!", "Nimic nu expiră curând. Revino mai târziu!"],
        ["endingSoon.until"] = ["until", "până la"],
        ["endingSoon.view"] = ["View", "Vezi"],

        ["loyalty.earned"] = ["You earned a reward!", "Ai câștigat o recompensă!"],
        ["loyalty.text1"] = ["You completed 3 orders — claim a", "Ai finalizat 3 comenzi — revendică o"],
        ["loyalty.discount20"] = ["20% discount", "reducere de 20%"],
        ["loyalty.text2"] = ["and use it on any basket order whenever you want.", "și folosește-o la orice comandă din coș, oricând vrei."],
        ["loyalty.claim"] = ["Claim my 20% off", "Revendică reducerea de 20%"],
        ["loyalty.claimed"] = ["20% Discount Claimed! Apply it from your basket on any future order.", "Reducere de 20% revendicată! Aplic-o din coș la orice comandă viitoare."],
        ["loyalty.claimFailed"] = ["We Couldn't Claim Your Reward. Please try again.", "Nu am putut revendica recompensa. Încearcă din nou."],

        ["login.title"] = ["Welcome back", "Bine ai revenit"],
        ["login.verifyMyEmail"] = ["Verify my email", "Verifică-mi emailul"],
        ["login.noAccount"] = ["Don't have an account?", "Nu ai cont?"],
        ["login.registerLink"] = ["Register", "Înregistrează-te"],

        ["common.email"] = ["Email", "Email"],
        ["common.password"] = ["Password", "Parolă"],

        ["manage.title"] = ["Manage Businesses", "Administrare localuri"],
        ["manage.addBusiness"] = ["Add Business", "Adaugă local"],

        ["notFound.title"] = ["Not Found", "Pagină negăsită"],
        ["notFound.text"] = ["Sorry, the content you are looking for does not exist.", "Ne pare rău, conținutul căutat nu există."],

        ["search.placeholder"] = ["Search food or places...", "Caută mâncare sau localuri..."],

        ["city.searchPlaceholder"] = ["Search your city...", "Caută orașul tău..."],
        ["city.selected"] = ["Selected city", "Oraș selectat"],

        ["card.details"] = ["Details", "Detalii"],
        ["card.deleting"] = ["Deleting...", "Se șterge..."],
        ["card.deleted"] = ["Business Deleted Successfully!", "Local șters cu succes!"],
        ["card.deleteFailed"] = ["We Couldn't Delete This Business. Please try again.", "Nu am putut șterge acest local. Încearcă din nou."],

        ["manage.noBusinesses"] = ["No businesses found.", "Niciun local găsit."],

        ["pkgForm.addTitle"] = ["Add Package", "Adaugă pachet"],
        ["pkgForm.editTitle"] = ["Edit Package", "Editează pachetul"],
        ["pkgForm.namePlaceholder"] = ["Add Package Name", "Numele pachetului"],
        ["pkgForm.description"] = ["Description", "Descriere"],
        ["pkgForm.image"] = ["Image", "Imagine"],
        ["pkgForm.extraPhotos"] = ["Extra photos (optional)", "Poze suplimentare (opțional)"],
        ["pkgForm.quantity"] = ["Quantity", "Cantitate"],
        ["pkgForm.startPickup"] = ["Start Pick-up", "Început ridicare"],
        ["pkgForm.endPickup"] = ["End Pick-up", "Sfârșit ridicare"],
        ["pkgForm.discountPlan"] = ["Discount plan", "Plan de reducere"],
        ["pkgForm.noDiscount"] = ["No discount", "Fără reducere"],
        ["pkgForm.immediateDiscount"] = ["Discounted from the start", "Redus de la început"],
        ["pkgForm.scheduledDiscount"] = ["Discount in the last X hours before pickup ends", "Reducere în ultimele X ore înainte de finalul ridicării"],
        ["pkgForm.discountPercent"] = ["Discount (%)", "Reducere (%)"],
        ["pkgForm.hoursBefore"] = ["How many hours before the pickup deadline?", "Cu câte ore înainte de termenul de ridicare?"],
        ["pkgForm.scheduledHint"] = ["When that moment comes, the price drops automatically and the package shows up with the discount badge (and on Ending Soon once under an hour remains).", "Când vine acel moment, prețul scade automat și pachetul apare cu eticheta de reducere (și pe Expiră curând când rămâne sub o oră)."],
        ["pkgForm.packageType"] = ["Package Type", "Tipul pachetului"],
        ["pkgForm.pickType"] = ["Pick Type...", "Alege tipul..."],
        ["pkgForm.uploadFailed"] = ["Image upload failed. Use a jpg, png or webp under 5 MB.", "Încărcarea imaginii a eșuat. Folosește jpg, png sau webp sub 5 MB."],
        ["pkgForm.saveFailed"] = ["Saving the package failed. Please try again.", "Salvarea pachetului a eșuat. Încearcă din nou."],

        ["common.saveChanges"] = ["Save Changes", "Salvează modificările"],

        ["bizForm.backToManage"] = ["Back to Manage", "Înapoi la Administrare"],
        ["bizForm.businessType"] = ["Business Type", "Tipul localului"],
        ["bizForm.ownerEmail"] = ["Owner email (optional, makes that user a partner)", "Emailul proprietarului (opțional, îl face partener)"],
        ["bizForm.saveFailed"] = ["Saving the business failed. Please try again.", "Salvarea localului a eșuat. Încearcă din nou."],

        ["register.title"] = ["Create your account", "Creează-ți contul"],
        ["register.confirmPassword"] = ["Confirm Password", "Confirmă parola"],
        ["register.contactInfo"] = ["Contact Info", "Date de contact"],
        ["register.haveAccount"] = ["Already have an account?", "Ai deja cont?"],

        ["common.name"] = ["Name", "Nume"],
        ["common.city"] = ["City", "Oraș"],

        ["verify.title"] = ["Verify email", "Verificare email"],
        ["verify.heading"] = ["Verify your email", "Verifică-ți emailul"],
        ["verify.sentTo"] = ["We sent a 6-digit code to", "Ți-am trimis un cod de 6 cifre la"],
        ["verify.enterBelow"] = ["Enter it below to activate your account.", "Introdu-l mai jos ca să-ți activezi contul."],
        ["verify.codeLabel"] = ["Verification code", "Cod de verificare"],
        ["verify.verifying"] = ["Verifying...", "Se verifică..."],
        ["verify.button"] = ["Verify email", "Verifică emailul"],
        ["verify.didntGet"] = ["Didn't get the code?", "Nu ai primit codul?"],
        ["verify.resend"] = ["Send a new one", "Trimite unul nou"],
        ["verify.newCodeSent"] = ["A new code is on its way. Check your inbox.", "Un cod nou e pe drum. Verifică-ți inboxul."],

        ["basket.title"] = ["My basket", "Coșul meu"],
        ["basket.empty"] = ["Your basket is empty. Add packages from a place you like — you can order several at once.", "Coșul tău e gol. Adaugă pachete de la un local care îți place — poți comanda mai multe deodată."],
        ["basket.browsePlaces"] = ["Browse places", "Vezi localurile"],
        ["basket.orderingFrom"] = ["Ordering from", "Comanzi de la"],
        ["basket.applyLoyalty"] = ["Apply your 20% loyalty discount to this order", "Aplică reducerea ta de fidelitate de 20% la această comandă"],
        ["basket.total"] = ["Total", "Total"],
        ["basket.placing"] = ["Placing order...", "Se plasează comanda..."],
        ["basket.placeOrder"] = ["Place order", "Plasează comanda"],
        ["basket.clear"] = ["Clear", "Golește"],
        ["basket.orderPlaced"] = ["Order Placed Successfully! {0} package(s) reserved — track them under My Orders.", "Comandă plasată cu succes! {0} pachet(e) rezervat(e) — le găsești la Comenzile mele."],
        ["basket.orderFailed"] = ["We Couldn't Place Your Order. Please make sure you're logged in and that all items in your basket are still available.", "Nu am putut plasa comanda. Asigură-te că ești autentificat și că toate produsele din coș sunt încă disponibile."],

        ["orders.title"] = ["My orders", "Comenzile mele"],
        ["orders.empty"] = ["You have no orders yet.", "Nu ai încă nicio comandă."],
        ["orders.findDeal"] = ["Find a deal", "Găsește o ofertă"],
        ["orders.notPickedUp"] = ["Not picked up in time", "Neridicată la timp"],
        ["orders.loyaltyBadge"] = ["loyalty", "fidelitate"],
        ["orders.cancel"] = ["Cancel order", "Anulează comanda"],
        ["orders.cancelled"] = ["Order Cancelled Successfully!", "Comandă anulată cu succes!"],
        ["orders.cancelFailed"] = ["We Couldn't Cancel This Order. Please try again.", "Nu am putut anula această comandă. Încearcă din nou."],

        ["status.new"] = ["New", "Nouă"],
        ["status.confirmed"] = ["Confirmed", "Confirmată"],
        ["status.pickedup"] = ["PickedUp", "Ridicată"],
        ["status.cancelled"] = ["Cancelled", "Anulată"],

        ["browse.backToCategories"] = ["Back to categories", "Înapoi la categorii"],
        ["browse.allLocations"] = ["Showing all locations", "Se afișează toate locațiile"],
        ["browse.placesIn"] = ["Showing places in {0}", "Localuri din {0}"],
        ["browse.noPlaces"] = ["No places in this category yet.", "Încă nu există localuri în această categorie."],
        ["browse.noPlacesIn"] = ["No places in this category in {0} yet.", "Încă nu există localuri în această categorie în {0}."],
        ["browse.tryAnother"] = ["Try another category", "Încearcă altă categorie"],
        ["browse.viewOrder"] = ["View & Order", "Vezi și comandă"],
        ["browse.loadMore"] = ["Load more", "Încarcă mai multe"],

        ["common.loading"] = ["Loading...", "Se încarcă..."],

        ["fav.updateFailed"] = ["We Couldn't Update Your Favorites. Please try again.", "Nu am putut actualiza favoritele. Încearcă din nou."],
        ["fav.added"] = ["Added to Your Favorites!", "Adăugat la favorite!"],
        ["fav.removed"] = ["Removed from Your Favorites.", "Eliminat din favorite."],

        ["order.heroTitle"] = ["What are you craving?", "Ce pofte ai azi?"],
        ["order.heroText"] = ["Pick a category and discover discounted meals near you.", "Alege o categorie și descoperă mâncare cu reducere lângă tine."],
        ["order.fastFoodDesc"] = ["Burgers, pizza, quick bites", "Burgeri, pizza, gustări rapide"],
        ["order.bakeryDesc"] = ["Pastries, bread, sweets", "Patiserie, pâine, dulciuri"],
        ["order.fineDiningDesc"] = ["Restaurant-quality meals", "Preparate de restaurant"],
        ["order.recommended"] = ["Recommended for you", "Recomandat pentru tine"],
        ["order.bandTitle"] = ["Fresh food, better prices", "Mâncare proaspătă, prețuri mai bune"],
        ["order.bandText"] = ["All our partners follow strict freshness standards — and every order helps reduce food waste.", "Toți partenerii noștri respectă standarde stricte de prospețime — și fiecare comandă ajută la reducerea risipei alimentare."],

        ["fav.myFavorites"] = ["My favorites", "Favoritele mele"],
        ["fav.empty"] = ["You have no favorite places yet. Tap the heart on a place to save it here.", "Nu ai încă localuri favorite. Apasă pe inimă la un local ca să-l salvezi aici."],

        ["confirm.title"] = ["Order confirmed", "Comandă confirmată"],
        ["confirm.notFound"] = ["We could not find this order.", "Nu am găsit această comandă."],
        ["confirm.goToOrders"] = ["Go to My Orders", "Mergi la Comenzile mele"],
        ["confirm.placed"] = ["Order placed!", "Comandă plasată!"],
        ["confirm.reserved"] = ["Your food is reserved. Show this order at pickup.", "Mâncarea ta e rezervată. Arată această comandă la ridicare."],
        ["confirm.from"] = ["from", "de la"],
        ["confirm.orderNo"] = ["Order #", "Comanda #"],
        ["confirm.keepBrowsing"] = ["Keep browsing", "Continuă să explorezi"],

        ["common.back"] = ["Back", "Înapoi"],
        ["common.edit"] = ["Edit", "Editează"],
        ["common.delete"] = ["Delete", "Șterge"],
        ["common.save"] = ["Save", "Salvează"],

        ["details.editBusiness"] = ["Edit Business", "Editează localul"],
        ["details.saved"] = ["Saved", "Salvat"],
        ["details.save"] = ["Save", "Salvează"],
        ["details.type"] = ["Type", "Tip"],
        ["details.location"] = ["Location", "Locație"],
        ["details.contact"] = ["Contact", "Contact"],
        ["details.address"] = ["Address", "Adresă"],
        ["details.packages"] = ["Packages", "Pachete"],
        ["details.addPackage"] = ["Add Package", "Adaugă pachet"],
        ["details.noPackages"] = ["No packages yet.", "Încă nu există pachete."],
        ["details.left"] = ["left", "rămase"],
        ["details.order"] = ["Order", "Comandă"],
        ["details.reviews"] = ["Reviews", "Recenzii"],
        ["details.noReviews"] = ["No reviews yet. Be the first to leave one!", "Încă nu există recenzii. Fii primul care lasă una!"],
        ["details.anonymous"] = ["Anonymous", "Anonim"],
        ["details.leaveReview"] = ["Leave a review", "Lasă o recenzie"],
        ["details.stars"] = ["Stars", "Stele"],
        ["details.loginToReview"] = ["to leave a review.", "ca să lași o recenzie."],
        ["details.reviewAfterPickup"] = ["You can leave a review once your order from this place has been picked up.", "Poți lăsa o recenzie după ce ai ridicat o comandă de la acest local."],
        ["details.placeThisOrder"] = ["Place this order?", "Plasezi această comandă?"],
        ["details.orderConfirmText"] = ["Are you sure you want to order {0} for {1}? It will be reserved for you and removed from the list.", "Sigur vrei să comanzi {0} la prețul de {1}? Va fi rezervat pentru tine și scos din listă."],
        ["details.yesOrder"] = ["Yes, order it", "Da, comandă"],

        ["city.title"] = ["Change your city", "Schimbă orașul"],
        ["city.current"] = ["Current city", "Orașul curent"],
        ["city.notSet"] = ["not set", "nesetat"],
        ["city.new"] = ["New city", "Oraș nou"],
        ["city.changed"] = ["City Changed Successfully to {0}!", "Orașul a fost schimbat cu succes în {0}!"],
        ["city.failed"] = ["We couldn't change your city. Please try again.", "Nu am putut schimba orașul. Încearcă din nou."],

        ["profile.title"] = ["My Profile", "Profilul meu"],
        ["profile.needLogin"] = ["You need to log in to see your profile.", "Trebuie să te autentifici ca să-ți vezi profilul."],
        ["profile.account"] = ["Account", "Cont"],
        ["profile.change"] = ["Change", "Schimbă"],
        ["profile.myRating"] = ["My rating as customer", "Nota mea ca client"],
        ["profile.ratingsFrom"] = ["{0} rating(s) from businesses", "{0} evaluări de la localuri"],
        ["profile.noRatings"] = ["no ratings yet — businesses rate you after each pickup", "nicio evaluare încă — localurile te evaluează după fiecare ridicare"],
        ["profile.personal"] = ["Personal details", "Date personale"],
        ["profile.contactPhone"] = ["Contact (phone)", "Contact (telefon)"],
        ["profile.saveChanges"] = ["Save changes", "Salvează modificările"],
        ["profile.preferences"] = ["Food preferences", "Preferințe culinare"],
        ["profile.preferencesText"] = ["Pick your favorite categories — we'll highlight them when you order.", "Alege categoriile tale preferate — le vom evidenția când comanzi."],
        ["profile.savePreferences"] = ["Save preferences", "Salvează preferințele"],
        ["profile.changePassword"] = ["Change password", "Schimbă parola"],
        ["profile.currentPassword"] = ["Current password", "Parola actuală"],
        ["profile.newPassword"] = ["New password", "Parola nouă"],
        ["profile.confirmNewPassword"] = ["Confirm new password", "Confirmă parola nouă"],
        ["profile.myReviews"] = ["My reviews", "Recenziile mele"],
        ["profile.latestOf"] = ["(latest 5 of {0})", "(ultimele 5 din {0})"],
        ["profile.noReviews"] = ["You have not written any reviews yet.", "Nu ai scris încă nicio recenzie."],
        ["profile.updated"] = ["Profile Updated Successfully!", "Profil actualizat cu succes!"],
        ["profile.updateFailed"] = ["We Couldn't Update Your Profile. Please try again.", "Nu am putut actualiza profilul. Încearcă din nou."],
        ["profile.prefsSaved"] = ["Preferences Saved Successfully!", "Preferințe salvate cu succes!"],
        ["profile.prefsFailed"] = ["We Couldn't Save Your Preferences. Please try again.", "Nu am putut salva preferințele. Încearcă din nou."],
        ["profile.passwordMismatch"] = ["The new passwords do not match.", "Parolele noi nu coincid."],
        ["profile.passwordChanged"] = ["Password Changed Successfully!", "Parolă schimbată cu succes!"],
        ["profile.reviewDeleted"] = ["Review Deleted Successfully!", "Recenzie ștearsă cu succes!"],
        ["profile.reviewDeleteFailed"] = ["We Couldn't Delete This Review. Please try again.", "Nu am putut șterge această recenzie. Încearcă din nou."],

        ["myBiz.noBusiness"] = ["No business is linked to your account yet. Contact the EcoMeal team to get onboarded.", "Niciun local nu e legat de contul tău încă. Contactează echipa EcoMeal pentru înrolare."],
        ["myBiz.myPackages"] = ["My packages", "Pachetele mele"],
        ["myBiz.noPackages"] = ["You have no packages yet. Add one to start selling.", "Nu ai încă pachete. Adaugă unul ca să începi să vinzi."],
        ["myBiz.addFirst"] = ["Add your first package", "Adaugă primul tău pachet"],
        ["myBiz.soldOut"] = ["Sold out", "Epuizat"],
        ["myBiz.pkgDeleted"] = ["Package Deleted Successfully!", "Pachet șters cu succes!"],
        ["myBiz.pkgDeleteFailed"] = ["We Couldn't Delete This Package. Please try again.", "Nu am putut șterge acest pachet. Încearcă din nou."],

        ["bizOrders.empty"] = ["No orders received yet.", "Nicio comandă primită încă."],
        ["bizOrders.package"] = ["Package", "Pachet"],
        ["bizOrders.price"] = ["Price", "Preț"],
        ["bizOrders.customer"] = ["Customer", "Client"],
        ["bizOrders.date"] = ["Date", "Data"],
        ["bizOrders.status"] = ["Status", "Stare"],
        ["bizOrders.actions"] = ["Actions", "Acțiuni"],
        ["bizOrders.noRatingsYet"] = ["No ratings from businesses yet", "Nicio evaluare de la localuri încă"],
        ["bizOrders.newCustomer"] = ["new", "nou"],
        ["bizOrders.confirm"] = ["Confirm", "Confirmă"],
        ["bizOrders.pickedUp"] = ["Picked up", "Ridicată"],
        ["bizOrders.statusUpdated"] = ["Order Status Updated Successfully!", "Starea comenzii a fost actualizată cu succes!"],
        ["bizOrders.statusFailed"] = ["We Couldn't Update This Order. Please try again in a moment.", "Nu am putut actualiza această comandă. Încearcă din nou imediat."],

        ["bizStats.title"] = ["Sales", "Vânzări"],
        ["bizStats.revenue"] = ["Revenue (picked up)", "Venit (comenzi ridicate)"],
        ["bizStats.totalOrders"] = ["Total orders", "Total comenzi"],
        ["bizStats.pickedUp"] = ["Picked up", "Ridicate"],
        ["bizStats.cancelled"] = ["Cancelled", "Anulate"],
        ["bizStats.revenueByMonth"] = ["Revenue — last 6 months", "Venit — ultimele 6 luni"],
        ["bizStats.noPickedUp"] = ["No picked-up orders yet.", "Nicio comandă ridicată încă."],
        ["bizStats.topPackages"] = ["Top packages", "Top pachete"],
        ["bizStats.noOrders"] = ["No orders yet.", "Nicio comandă încă."],
        ["bizStats.ordersWord"] = ["orders", "comenzi"],

        ["partner.title"] = ["Become a Partner", "Devino partener"],
        ["partner.heroTitle"] = ["Become an EcoMeal partner", "Devino partener EcoMeal"],
        ["partner.heroText"] = ["Turn surplus food into revenue and reach eco-conscious customers in your city.", "Transformă surplusul de mâncare în venit și ajungi la clienți eco-conștienți din orașul tău."],
        ["partner.howToJoin"] = ["How to join", "Cum te alături"],
        ["partner.threeSteps"] = ["Three simple steps", "Trei pași simpli"],
        ["partner.step1Title"] = ["Send us an email", "Trimite-ne un email"],
        ["partner.step1Text"] = ["Tell us you'd like to join at", "Spune-ne că vrei să te alături la"],
        ["partner.step2Title"] = ["Present your business", "Prezintă-ți localul"],
        ["partner.step2Text"] = ["Share your name, city, type of food and the kind of surplus you usually have.", "Spune-ne numele, orașul, tipul de mâncare și ce surplus ai de obicei."],
        ["partner.step3Title"] = ["Get onboarded", "Ești adăugat"],
        ["partner.step3Text"] = ["We review, approve and add your restaurant to the app.", "Analizăm, aprobăm și adăugăm restaurantul tău în aplicație."],
        ["partner.requirements"] = ["Requirements", "Cerințe"],
        ["partner.req1"] = ["Only fresh, safe-to-eat food — no expired products.", "Doar mâncare proaspătă și sigură — fără produse expirate."],
        ["partner.req2"] = ["Valid business registration and food-safety authorization.", "Firmă înregistrată și autorizație sanitar-veterinară valide."],
        ["partner.req3"] = ["Accurate pickup times and honest package descriptions.", "Intervale de ridicare corecte și descrieri oneste ale pachetelor."],
        ["partner.req4"] = ["Discounted prices compared to the original menu.", "Prețuri reduse față de meniul original."],
        ["partner.req5"] = ["At least one physical location in a supported city.", "Cel puțin o locație fizică într-un oraș acoperit."],
        ["partner.emailUs"] = ["Email us at", "Scrie-ne la"],

        ["adminStats.title"] = ["Statistics", "Statistici"],
        ["adminStats.adminsOnly"] = ["Statistics are only available for admins.", "Statisticile sunt disponibile doar pentru administratori."],
        ["adminStats.ordersTotal"] = ["Orders (total)", "Comenzi (total)"],
        ["adminStats.foodSaved"] = ["Food saved from waste", "Mâncare salvată de la risipă"],
        ["adminStats.avgOrder"] = ["Average order value", "Valoare medie comandă"],
        ["adminStats.partners"] = ["Partners", "Parteneri"],
        ["adminStats.locations"] = ["Business locations", "Locații"],
        ["adminStats.packagesAvailable"] = ["Packages available", "Pachete disponibile"],
        ["adminStats.reviewsAvg"] = ["Reviews · avg rating", "Recenzii · nota medie"],
        ["adminStats.last7Days"] = ["Orders in the last 7 days", "Comenzi în ultimele 7 zile"],
        ["adminStats.noOrders7"] = ["No orders in the last 7 days.", "Nicio comandă în ultimele 7 zile."],
        ["adminStats.day"] = ["Day", "Ziua"],
        ["adminStats.orders"] = ["Orders", "Comenzi"],
        ["adminStats.topBusinesses"] = ["Top businesses", "Top localuri"],
        ["adminStats.business"] = ["Business", "Local"],
        ["adminStats.cities"] = ["Cities", "Orașe"],
        ["adminStats.noBusinesses"] = ["No businesses yet.", "Niciun local încă."],
        ["adminStats.businesses"] = ["Businesses", "Localuri"],

        ["adminUsers.empty"] = ["No users found (are you logged in as admin?).", "Niciun utilizator găsit (ești autentificat ca admin?)."],
        ["adminUsers.count"] = ["{0} registered users. Emails are ready for future campaigns.", "{0} utilizatori înregistrați. Emailurile sunt pregătite pentru campanii viitoare."],
        ["adminUsers.account"] = ["Account", "Cont"],
        ["adminUsers.emailVerified"] = ["Email verified", "Email verificat"],
        ["adminUsers.business"] = ["Business", "Local"],
        ["adminUsers.locationsWord"] = ["locations", "locații"],
        ["adminUsers.customer"] = ["Customer", "Client"],
        ["adminUsers.verified"] = ["Verified", "Verificat"],
        ["adminUsers.notVerified"] = ["Not verified", "Neverificat"],
    };
}
