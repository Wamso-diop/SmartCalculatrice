[![Aperçu de SmartCalculator](./Resources/Images/preview.png)]


# SmartCalculator

Calculatrice mobile développée avec **.NET MAUI**, dans le cadre de l'Activité n°4 — Atelier de développement Mobile.

Application native Android combinant un mode standard et un mode scientifique, une mémoire, un historique des calculs, et une interface entièrement responsive qui s'adapte à la rotation de l'écran sans déformation ni défilement.

## Sommaire

- [Aperçu](#aperçu)
- [Fonctionnalités](#fonctionnalités)
- [Layouts utilisés](#layouts-utilisés)
- [Stack technique](#stack-technique)
- [Structure du projet](#structure-du-projet)
- [Installation et exécution](#installation-et-exécution)
- [Tests réalisés](#tests-réalisés)
- [Auteur](#auteur)

## Aperçu

<!-- Remplace ce bloc par une ou deux captures d'écran réelles de l'app, en portrait et en paysage -->
<!-- ![Mode standard](docs/screenshot-standard.png) -->
<!-- ![Mode scientifique](docs/screenshot-scientifique.png) -->

## Fonctionnalités

### Opérations de base

- Addition, soustraction, multiplication, division
- Saisie de nombres décimaux
- Remise à zéro totale (`AC`) et effacement du dernier caractère (`⌫`)
- Changement de signe (`±`) et pourcentage (`%`)
- Division par zéro interceptée et affichée sous forme de message contrôlé, sans plantage de l'application
- Affichage de l'opération en cours au-dessus du résultat, avec **évaluation du résultat partiel en temps réel** pendant la saisie

### Fonctions avancées

- **Mode scientifique** : √, x², xʸ, 1/x, |x|, n!, sin/cos/tan et leurs réciproques, log, ln, 10ˣ, eˣ, π, e, parenthèses, bascule DEG/RAD
- **Mémoire** : MC, MR, M+, M−, MS
- **Historique** des calculs, consultable et réutilisable par simple sélection
- **Interface responsive** : deux dispositions distinctes (portrait / paysage), recalculées dynamiquement à la rotation, sans `ScrollView`

## Layouts utilisés

Conformément à la contrainte du sujet, la page combine simultanément **quatre types de layouts** différents :

| Layout | Emplacement | Justification |
|---|---|---|
| `Grid` | Structure générale, clavier numérique, pavé scientifique, pavé mémoire | Seul layout permettant d'aligner précisément boutons et lignes/colonnes proportionnelles, et de recalculer la disposition entre portrait et paysage |
| `HorizontalStackLayout` | Barre d'en-tête (titre, indicateur mémoire, bascule DEG/RAD, bouton historique) | Aligne naturellement des éléments de tailles différentes sur une seule ligne |
| `VerticalStackLayout` | Contenu de l'écran d'affichage (expression puis résultat), panneau d'historique | Empile verticalement un nombre variable d'éléments sans fixer de lignes à l'avance |
| `Border` | Cadre de l'écran d'affichage, cartes de l'historique | Encadrement visuel (bordure, coins arrondis, fond) autour d'un bloc de contenu |

## Stack technique

- **.NET 8** / **.NET MAUI**
- **C#** pour la logique métier et le code-behind
- **XAML** pour l'interface
- Architecture en couches : `Models/`, `Services/`, `MainPage`

## Structure du projet

```
SmartCalculator/
│
├── MainPage.xaml           # Interface : header, écran, pavé scientifique, clavier, historique
├── MainPage.xaml.cs        # Gestion des événements, placement dynamique portrait/paysage
│
├── Models/
│   └── CalculatorState.cs  # Enum AngleMode, modèle HistoryEntry
│
├── Services/
│   ├── CalculatorEngine.cs # Moteur de calcul : parsing, évaluation, gestion d'erreurs
│   ├── MemoryService.cs    # Logique de la mémoire (MC/MR/M+/M−/MS)
│   └── HistoryService.cs   # Gestion de l'historique des calculs
│
├── Resources/
│   ├── Styles/
│   │   └── Styles.xaml     # Palette de couleurs, styles des boutons
│   └── Fonts/
│
└── App.xaml
```

Le moteur de calcul (`CalculatorEngine`) est totalement indépendant de l'interface : toute la logique mathématique (conversion en notation postfixée, priorité des opérateurs, fonctions scientifiques, gestion des erreurs) peut être testée séparément du XAML.

## Installation et exécution

### Prérequis

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download) avec le workload MAUI :
  ```bash
  dotnet workload install maui-android
  ```
- Un appareil Android connecté (débogage USB activé) ou un émulateur

### Lancer le projet

```bash
git clone https://github.com/<ton-compte>/SmartCalculator.git
cd SmartCalculator
dotnet build -f net8.0-android
dotnet build -t:Run -f net8.0-android
```

## Tests réalisés

L'application a été testée en conditions réelles sur un smartphone Android (Tecno Spark 7), notamment :

- Les quatre opérations de base, avec décimales et parenthèses
- Le pourcentage en contexte simple et composé (ex. `200 + 10%`)
- Les fonctions scientifiques (racine, puissance, trigonométrie, logarithmes)
- La mémoire (stockage, rappel, ajout, soustraction, effacement)
- Les cas d'erreur : division par zéro, racine négative, logarithme indéfini, factorielle invalide, expression incomplète
- Le changement d'orientation répété (portrait ↔ paysage), sans perte d'affichage ni déformation

## Auteur

**LONTSIE YEMAGOU Boris**
Activité n°4 — Atelier de développement Mobile