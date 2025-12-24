# PDF Oluşturma Talimatları

## Yöntem 1: Pandoc ile PDF (Önerilen)

### 1. Pandoc Kurulumu

```bash
# macOS
brew install pandoc
brew install basictex  # LaTeX for PDF generation

# Windows
# https://pandoc.org/installing.html adresinden installer indir
```

### 2. PDF Oluştur

```bash
cd "/Users/sudedincer/Desktop/bomberman 2/Bomberman"

# Basit PDF
pandoc DESIGN_DOCUMENT.md -o DESIGN_DOCUMENT.pdf

# Gelişmiş PDF (table of contents, syntax highlighting)
pandoc DESIGN_DOCUMENT.md \
  -o DESIGN_DOCUMENT.pdf \
  --toc \
  --toc-depth=3 \
  --highlight-style=tango \
  --metadata title="Bomberman Multiplayer - Design Document" \
  --metadata author="Sude Dincer" \
  --metadata date="December 2024"
```

---

## Yöntem 2: GitHub Preview + Print to PDF

1. **DESIGN_DOCUMENT.md dosyasını GitHub'a push et**
2. **GitHub'da aç** - Mermaid diyagramları otomatik render olur
3. **Browser'da Print** (Cmd+P)
4. **Save as PDF** seç

Bu yöntem Mermaid diyagramlarını görselleştirir.

---

## Yöntem 3: Markdown to HTML + Print

```bash
# pandoc ile HTML oluştur
pandoc DESIGN_DOCUMENT.md -o DESIGN_DOCUMENT.html --standalone

# HTML'i browser'da aç
open DESIGN_DOCUMENT.html

# Print to PDF (Cmd+P)
```

**Not:** Mermaid diyagramları HTML'de gösterilmez, GitHub kullan.

---

## Yöntem 4: VS Code Extension

1. **VS Code'da Markdown Preview Enhanced extension kur**
2. **DESIGN_DOCUMENT.md aç**
3. **Right-click → Markdown Preview Enhanced: Open Preview**
4. **Preview'da Right-click → Export to PDF**

Mermaid diyagramlarını render eder.

---

## Önerilen Akış

```bash
# 1. Pandoc kur (tek seferlik)
brew install pandoc
brew install basictex

# 2. PDF oluştur
cd "/Users/sudedincer/Desktop/bomberman 2/Bomberman"
pandoc DESIGN_DOCUMENT.md -o DESIGN_DOCUMENT.pdf \
  --toc \
  --metadata title="Bomberman Multiplayer Design Document"

# 3. PDF görüntüle
open DESIGN_DOCUMENT.pdf
```

**Alternatif:** Design document'i olduğu gibi GitHub'a yükle, orada Mermaid diyagramları otomatik render olur!
