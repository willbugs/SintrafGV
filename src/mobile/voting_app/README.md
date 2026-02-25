# SintrafGV - App Nativo de Votação (Flutter + WebView)

App nativo Android/iOS que carrega o PWA de votação via WebView.

## 🎯 Conceito

Este app Flutter funciona como um **wrapper nativo** para o PWA React de votação:

- **PWA já pronto**: `src/frontend/voting` (React + Vite + MUI)
- **App Flutter**: Carrega o PWA em WebView nativa
- **Benefício**: Não precisa reescrever código, apenas "empacota" o PWA

## 📱 Arquitetura

```
┌─────────────────────────────────────────┐
│   APP FLUTTER (Android/iOS)             │
│                                         │
│  ┌───────────────────────────────────┐ │
│  │       WebView Nativa              │ │
│  │                                   │ │
│  │  Carrega:                         │ │
│  │  https://votacao.sintrafgv.com.br │ │
│  │                                   │ │
│  │  ou http://localhost:5173 (dev)   │ │
│  └───────────────────────────────────┘ │
│                                         │
│  + Recursos Nativos:                    │
│    ✅ Câmera (QR Code)                  │
│    ✅ Notificações Push                 │
│    ✅ Biometria (futuro)                │
│    ✅ Instalável nas lojas              │
└─────────────────────────────────────────┘
```

## ✅ Funcionalidades Implementadas

### 1. **Splash Screen**
- Logo SintrafGV
- Loading animado
- 2 segundos de duração

### 2. **WebView Completa**
- Carrega PWA de votação
- JavaScript habilitado
- Navegação dentro do domínio permitida
- Voltar via botão nativo (Android)

### 3. **Conectividade**
- Monitoramento de internet em tempo real
- Indicador visual "Sem conexão"
- SnackBar de aviso

### 4. **Tratamento de Erros**
- Tela de erro personalizada
- Botão "Tentar novamente"
- Reload da página

### 5. **Loading Indicator**
- Mostra enquanto carrega
- Oculta após carregamento completo

## 🛠️ Tecnologias

### Dependências
- **flutter**: SDK oficial
- **webview_flutter**: ^4.10.0 - WebView nativa
- **connectivity_plus**: ^6.1.0 - Verificar conexão
- **url_launcher**: ^6.3.1 - Links externos
- **permission_handler**: ^11.3.1 - Permissões (câmera, etc)

### Permissões Android
```xml
✅ INTERNET
✅ ACCESS_NETWORK_STATE
✅ CAMERA
✅ WRITE_EXTERNAL_STORAGE (até API 32)
✅ READ_EXTERNAL_STORAGE (até API 32)
```

## 📂 Estrutura do Projeto

```
voting_app/
├── lib/
│   ├── main.dart                    # Entry point
│   └── screens/
│       ├── splash_screen.dart       # Tela inicial (2s)
│       └── webview_screen.dart      # WebView principal
├── android/                         # Configurações Android
├── ios/                             # Configurações iOS (futuro)
└── pubspec.yaml                     # Dependências
```

## 🚀 Desenvolvimento

### Pré-requisitos
- Flutter 3.32.6+
- Dart 3.8.1+
- Android Studio ou VS Code
- Emulador Android ou dispositivo físico

### Instalação
```bash
cd src/mobile/voting_app
flutter pub get
```

### Configurar URL do PWA

**Desenvolvimento (localhost):**

Editar `lib/screens/webview_screen.dart`:
```dart
// Android Emulator
static const String _pwaUrl = 'http://10.0.2.2:5173';

// iOS Simulator
static const String _pwaUrl = 'http://localhost:5173';
```

**Produção:**
```dart
static const String _pwaUrl = 'https://votacao.sintrafgv.com.br';
```

### Executar em Desenvolvimento

#### 1. Iniciar PWA (terminal 1)
```bash
cd src/frontend/voting
npm run dev
# PWA rodando em http://localhost:5173
```

#### 2. Executar App Flutter (terminal 2)
```bash
cd src/mobile/voting_app
flutter run
```

### Build para Produção

#### Android APK (Debug)
```bash
flutter build apk --debug
# Saída: build/app/outputs/flutter-apk/app-debug.apk
```

#### Android APK (Release)
```bash
flutter build apk --release
# Saída: build/app/outputs/flutter-apk/app-release.apk
```

#### Android App Bundle (Google Play)
```bash
flutter build appbundle --release
# Saída: build/app/outputs/bundle/release/app-release.aab
```

#### iOS (macOS apenas)
```bash
flutter build ios --release
```

## 📱 Instalação no Celular

### Via USB (Development)
```bash
# Conectar celular via USB com depuração ativada
flutter install
```

### Via APK
1. Build APK: `flutter build apk --release`
2. Transferir APK para celular
3. Instalar APK (permitir instalação de fontes desconhecidas)

## 🔧 Configurações Importantes

### 1. Nome do App
**Android:** `android/app/src/main/AndroidManifest.xml`
```xml
android:label="SintrafGV Votação"
```

**iOS:** `ios/Runner/Info.plist`
```xml
<key>CFBundleName</key>
<string>SintrafGV Votação</string>
```

### 2. Ícone do App
Substituir ícones em:
- `android/app/src/main/res/mipmap-*/ic_launcher.png`
- `ios/Runner/Assets.xcassets/AppIcon.appiconset/`

Usar ferramenta: https://appicon.co/

### 3. Orientação da Tela
**main.dart** (já configurado):
```dart
SystemChrome.setPreferredOrientations([
  DeviceOrientation.portraitUp,
  DeviceOrientation.portraitDown,
]);
```

### 4. Cleartext Traffic (HTTP localhost)
**AndroidManifest.xml** (já configurado):
```xml
android:usesCleartextTraffic="true"
```

## 🎨 Personalização

### Cores do Tema
**main.dart:**
```dart
primaryColor: const Color(0xFF1976d2), // Azul SintrafGV
```

### Splash Screen
**lib/screens/splash_screen.dart:**
- Logo: Icon(Icons.how_to_vote)
- Cor de fundo: Color(0xFF1976d2)
- Duração: 2 segundos

Substituir por imagem real:
```dart
Image.asset('assets/logo.png', width: 120, height: 120)
```

## 🐛 Debug

### Ver logs
```bash
flutter logs
```

### Inspecionar WebView
**Chrome DevTools (Android):**
1. Abrir Chrome: `chrome://inspect`
2. Conectar dispositivo USB
3. Clicar em "Inspect" na WebView

### Erros comuns

#### 1. "Erro ao carregar"
- ✅ Verificar se PWA está rodando (localhost:5173)
- ✅ Verificar URL no `webview_screen.dart`
- ✅ Verificar permissão INTERNET no AndroidManifest.xml

#### 2. "Sem conexão"
- ✅ Verificar WiFi/dados móveis
- ✅ Verificar firewall
- ✅ Para emulador: usar 10.0.2.2 ao invés de localhost

#### 3. WebView em branco
- ✅ Habilitar `android:usesCleartextTraffic="true"` para HTTP
- ✅ Verificar logs: `flutter logs`

## 🚀 Publicação

### Google Play Store

#### 1. Gerar keystore
```bash
keytool -genkey -v -keystore ~/upload-keystore.jks -keyalg RSA \
        -keysize 2048 -validity 10000 -alias upload
```

#### 2. Configurar assinatura
Criar `android/key.properties`:
```properties
storePassword=<senha>
keyPassword=<senha>
keyAlias=upload
storeFile=<caminho>/upload-keystore.jks
```

#### 3. Build release
```bash
flutter build appbundle --release
```

#### 4. Upload
- Acessar Google Play Console
- Criar novo app
- Upload `app-release.aab`

### Apple App Store (iOS)

Requer:
- macOS
- Xcode
- Apple Developer Account ($99/ano)
- Certificados e provisioning profiles

```bash
flutter build ios --release
# Abrir Xcode e fazer upload via Xcode
```

## 📊 Vantagens dessa Arquitetura

| Aspecto | PWA Puro | Flutter + WebView |
|---------|----------|------------------|
| Desenvolvimento | 1 código (React) | ✅ 1 código (React) + wrapper mínimo |
| Instalação | Atalho navegador | ✅ App real nas lojas |
| Recursos nativos | Limitado | ✅ Total acesso |
| Atualizações | Automáticas | ✅ Automáticas (WebView) |
| Offline | Service Worker | ✅ SW + cache nativo |
| SEO | Sim | Não aplicável |

## 📝 TODO / Melhorias Futuras

- [ ] Adicionar logo real (substituir ícone)
- [ ] Implementar notificações push (Firebase)
- [ ] Adicionar biometria para login
- [ ] Deep links (abrir enquete específica)
- [ ] Compartilhamento nativo (comprovante)
- [ ] Câmera para QR Code
- [ ] Cache offline avançado
- [ ] Splash screen animada
- [ ] Dark mode

## 🔗 Integração com Backend

O app carrega o PWA que se conecta com:
- **Backend .NET**: `http://localhost:5066/api`
- **Endpoints**:
  - `POST /api/auth/login-associado`
  - `GET /api/eleicoes`
  - `POST /api/votos`
  - `GET /api/votos/comprovante/{hash}`

## 📞 Suporte

- **Documentação Flutter**: https://docs.flutter.dev/
- **WebView Flutter**: https://pub.dev/packages/webview_flutter
- **Connectivity Plus**: https://pub.dev/packages/connectivity_plus

---

**✅ App Flutter pronto para desenvolvimento e publicação!**

*SintrafGV - Sistema de Votação Digital*  
*Versão 1.0.0 - Fevereiro 2026*
