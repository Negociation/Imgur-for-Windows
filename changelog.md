# Changelog

## Imgur UWP 2.5.0 — 2026 Release

### Description
Happy New Year folks 🎉

This is a **huge update** with a lot of new stuff — buckle up.
This will be the **last big update for now**. The app is entering **LTS (Long-Term Support) mode**, since I’m getting back to work after the holidays. From now on, expect only **small fixes** and completion of already-started pages.

---

### ✨ What's New
- Added **Search Page** with Tags, Posts, and Users working
- Added **Tag Page** (basic data only for now, with a small portion of items)
- Added **User Page** (basic data only for now)
- Added **Clipboard Listener (Preview)**
  - When an Imgur Gallery URL is copied, the app shows a notification with an option to open it inside the app
- Added **User Page hyperlink** from a Post
- Fixed multiple **missing translations** across the app
  - Translators: a new English resource file was added — feel free to translate and send it back
- **Major memory usage improvements** on Image Cards with videos
  - Especially beneficial for 1GB RAM devices
- Fixed a **possible memory leak**
  - In a specific scenario, the app was loading the original image as a thumbnail (e.g. 4K images)

---

### ⚠️ Known Issues
- Using the **hardware back button** can be erratic on mobile (already under investigation)
- Opening the **Navigation Bar** auto-focuses the AutoSuggestBox, covering footer items
- Using **Play/Pause transport controls** on media with multiple video elements behaves incorrectly

---

### 🔮 What's Next (LTS Mode — ordered by priority)
- Add **Custom API Key support** in Settings to extend the app’s lifespan
- Port **Imgur API Health Verification** from the old API
- Add standalone **Images, Tags, and Users opening** for the Clipboard Listener
- Add **Load More Items** for the Tag Page
- Add **Posts, Tags, and Favourites** to the User Page

---

### 🚫 Off the Table (for now)
- Load **Post Comments** *(requires proper frontend support)*
- **Login / Logout** *(OAuth2 on UWP — not implemented yet)*
- **Upload** *(requires login support)*
- **Share to App** *(requires upload support)*


## Imgur UWP 2.6.0 — LTS Release

### Description
Happy New Year folks 🎉, as mentioned before the app is entering on a LTS mode , due to the lack of time to build new stuff, the main focus now will be keep everything 
working as expected , and fix some current missing features from the current avaliable content.

### ✨ What's New
- Added Custom Api Key option on Settings (Big Feature)
- Added missing translations for the App
- Fixed a issue when the app was suspended, and after resume the user use the hardware back button it would suddenly close.
- Added Api Health verification on App Start, Ported from the Old Codebase, now if the Api is offline or slow the App will show a InApp Notification in order to avoid confusion.
- Added Albums , Tags and Users for the Clipboard Listner
- Fixed a Issue with Layout on Media for Larger Sizes including the Comments Section ( its not a promisse, but maybe... )
- Added support for Album Media Pages ( Usually Private with Less information )


### 🔮 What's Next (LTS Mode — ordered by priority)
- Add **Load More Items** for the Tag Page
- Add **Posts, Tags, and Favourites** to the User Page

### ⚠️ Known Issues
- Opening the **Navigation Bar** auto-focuses the AutoSuggestBox, covering footer items
- Using **Play/Pause transport controls** on media with multiple video elements behaves incorrectly

### 🚫 Off the Table (for now)
- Load **Post Comments** *(requires proper frontend support)*
- **Login / Logout** *(OAuth2 on UWP — not implemented yet)*
- **Upload** *(requires login support)*
- **Share to App** *(requires upload support)*