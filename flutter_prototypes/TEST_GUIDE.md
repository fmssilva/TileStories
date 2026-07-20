
# 🧪 TEST_GUIDE.md - Cross-Platform Testing

**Quick reference for testing TileStories app on Chrome and Android**

---

## 📋 PREREQUISITES

### Required Software:
1. **Flutter SDK** (3.41.0+) - https://flutter.dev/docs/get-started/install
2. **VS Code** - https://code.visualstudio.com/
3. **Chrome browser** (for web testing)
4. **Android Studio** (for Android testing) - https://developer.android.com/studio

### Optional:
- Physical Android device with USB debugging enabled

---

## 🌐 TESTING ON CHROME (Web)

### Setup (One-time):
```bash
# Verify Chrome is detected
flutter devices
# Output should show: Chrome (web) • chrome • web-javascript
```

### Run App:
```bash
# From project root
flutter run -d chrome
```

### Results:
- Launch time: ~24 seconds
- Hot reload: Press `r` (instant updates)
- DevTools: Auto-opens in browser
- Exit: Press `q` in terminal

### What to Test:
- ✅ POI loading (10 POIs from JSON)
- ✅ Language switching (PT ↔ EN)
- ✅ Zoom/pan (mouse scroll + drag)
- ✅ POI clicks (bottom sheet)
- ✅ Persistence (F5 refresh keeps language)

---

## 📱 TESTING ON ANDROID

### Setup (One-time):

#### 1. Install Android Studio:
- Download from https://developer.android.com/studio
- Run installer (accept all defaults)
- First launch: Install SDK components (~3-5 GB)
- Wait for downloads to complete

#### 2. Create Emulator:
**Option A: Via Android Studio GUI**
- Tools → Device Manager → Create Device
- Select: Pixel 6 (or any modern device)
- System Image: Android 13+ (API 33+)
- Download image → Finish

**Option B: Via Command Line**
```bash
# Check available emulators
flutter emulators

# Create new (if none exist)
flutter emulators --create

# Android Studio auto-created one for you if you see:
# Medium_Phone_API_36.1
```

#### 3. Accept Android Licenses:
```bash
flutter doctor --android-licenses
# Press 'y' to accept all
```

#### 4. Verify Setup:
```bash
flutter doctor -v
# Android toolchain should show ✅ (was ❌ before)
```

---

### Run App on Android:

#### Start Emulator:
```bash
# Launch emulator
flutter emulators --launch <emulator_id>
# Example: flutter emulators --launch Medium_Phone_API_36.1

# Wait 30-60 seconds for boot (emulator window opens)

# Verify ready
flutter devices
# Output should show: sdk gphone64 x86 64 (mobile) • emulator-5554
```

#### Build & Run:
```bash
# First run (downloads tools, builds APK)
flutter run -d emulator-5554
# Time: ~11-12 minutes (one-time download + build)

# Subsequent runs
flutter run -d emulator-5554
# Time: ~30-40 seconds (much faster!)
```

#### During Development:
- Hot reload: Press `r` (applies code changes instantly)
- Hot restart: Press `R` (full app restart)
- Quit: Press `q`
- Clear console: Press `c`

---

### What to Test on Android:

#### Core Features:
1. **App Launch** - Opens without crashes
2. **POI Loading** - All 10 POIs appear
3. **Touch Gestures:**
   - Pinch-to-zoom (two fingers)
   - Scroll-to-zoom (mouse wheel on emulator)
   - Drag-to-pan (one finger swipe)
4. **POI Interaction** - Tap marker → bottom sheet opens
5. **Language Switch** - Flag button → Select language
6. **Persistence** - Close app → Reopen → Language saved
7. **POI Visibility** - Eye icon toggles markers
8. **Rotation** - Portrait ↔ Landscape (Ctrl+F11/F12 on emulator)

#### Performance Check:
```bash
# Build release version (3-5x faster)
flutter build apk --release
# Output: build/app/outputs/flutter-apk/app-release.apk

# Install on emulator
flutter install -d emulator-5554

# Run
adb shell am start -n com.example.grande_panorama_ar/.MainActivity
```

---

## 🔍 DEBUGGING

### View Logs:
```bash
# While app is running
flutter logs

# Android-specific (detailed)
adb logcat | grep -i flutter
```

### Common Issues:

**Chrome: App won't load**
```bash
flutter clean
flutter pub get
flutter run -d chrome
```

**Android: Emulator won't start**
- Check BIOS: Enable Intel VT-x / AMD-V (virtualization)
- Windows: Disable Hyper-V in Windows Features
- Try x86_64 image instead of ARM

**Android: Build fails**
```bash
# Clear build cache
flutter clean
cd android
./gradlew clean  # Linux/Mac
gradlew.bat clean  # Windows
cd ..
flutter pub get
flutter run -d emulator-5554
```

**Android: "SDK not found"**
```bash
# Set Android SDK path
flutter config --android-sdk C:\Users\<USER>\AppData\Local\Android\Sdk
flutter doctor -v
```

---

## 📊 EXPECTED RESULTS

### Chrome (Web):
```
✅ Launch: 24s
✅ 10 POIs loaded
✅ Language: PT → EN working
✅ Zoom: Mouse scroll (0.32 → 0.83 scale)
✅ Pan: Click + drag
⚠️ Warning: Noto font (cosmetic, fixable)
```

### Android (Emulator):
```
✅ First build: ~12 min (downloads tools)
✅ Subsequent: ~30-40s
✅ 10 POIs loaded
✅ Language: PT → EN working
✅ Gestures: Pinch/scroll zoom, swipe pan
✅ POI tap: Bottom sheet opens
⚠️ Debug mode: Frame drops (normal)
   → Release build: Smooth 60 FPS
```

---

## ⚡ QUICK COMMANDS REFERENCE

### Environment:
```bash
flutter doctor -v          # Check setup
flutter devices            # List devices
flutter emulators          # List emulators
```

### Run:
```bash
flutter run -d chrome                    # Web
flutter run -d emulator-5554             # Android emulator
flutter run -d windows                   # Windows desktop
flutter run                              # Auto-select device
```

### Build:
```bash
flutter build web --release              # Web (output: build/web/)
flutter build apk --release              # Android APK
flutter build appbundle --release        # Android App Bundle (Play Store)
flutter build windows --release          # Windows .exe
```

### Maintenance:
```bash
flutter clean              # Clear build cache
flutter pub get            # Install dependencies
flutter pub upgrade        # Update packages
flutter pub outdated       # Check for updates
```

### Code Generation:
```bash
# Freezed + JSON serialization
flutter pub run build_runner build --delete-conflicting-outputs

# Watch mode (auto-regenerate on file change)
flutter pub run build_runner watch
```

---

## 🎯 COMPLETE TEST WORKFLOW

### Initial Setup (Day 1):
```bash
# 1. Verify Flutter
flutter doctor -v

# 2. Install Android Studio → Accept SDK downloads → Create emulator

# 3. Accept licenses
flutter doctor --android-licenses

# 4. Verify setup
flutter doctor -v  # All ✅
```

### Daily Testing (Day 2+):
```bash
# Terminal 1: Launch emulator
flutter emulators --launch Medium_Phone_API_36.1

# Terminal 2: Run app
cd TileStories_App
flutter run -d emulator-5554

# Test features manually in emulator window
# Press 'r' to hot reload after code changes
# Press 'q' to quit
```

### Before Commit:
```bash
# Test web
flutter run -d chrome
# Verify all features

# Test Android
flutter run -d emulator-5554
# Verify all features

# Build release
flutter build apk --release
flutter build web --release

# Verify no errors
flutter analyze
```

---

## 📱 PHYSICAL ANDROID DEVICE TESTING

### Setup:
1. **Enable Developer Options:**
   - Settings → About Phone → Tap "Build Number" 7x
2. **Enable USB Debugging:**
   - Settings → Developer Options → USB Debugging ON
3. **Connect USB cable**
4. **Authorize computer** (popup on phone)

### Verify:
```bash
adb devices
# Output: List of devices attached
#         ABC123456789    device
```

### Run:
```bash
flutter devices  # Should show your phone
flutter run      # Auto-deploys to phone
```

### Advantages:
- Real hardware performance
- True touch gestures
- GPS, sensors, camera testing
- No emulator lag

---

## 🎉 SUCCESS METRICS

### Chrome: ✅ Ready for deployment if:
- App loads in <30s
- All 10 POIs visible
- Language switching works
- No console errors (warnings OK)

### Android: ✅ Ready for deployment if:
- Release APK builds successfully
- App loads in <5s (release mode)
- 60 FPS performance
- All gestures responsive
- No crashes after 5-minute use

---

**Last Updated:** 2026-02-13  
**Flutter Version:** 3.41.0  
**Test Platforms:** Chrome, Android (API 36)  
**Status:** ✅ Both platforms validated
