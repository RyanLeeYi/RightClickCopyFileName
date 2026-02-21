# RightClickCopyFileName

Windows 右鍵選單工具，提供複製檔案名稱、檢查 DLL 位元版本，以及解譯 CS 檔案等實用功能。

## 功能說明

在檔案或資料夾上按右鍵後，會出現以下選單項目：

| 右鍵選單名稱 | 適用對象 | 功能說明 |
|---|---|---|
| 複製檔案名稱 | 所有檔案 | 將檔案名稱（不含路徑）複製到剪貼簿 |
| 複製檔案完整名稱 | 所有檔案 | 將檔案完整路徑複製到剪貼簿 |
| 檢查dll檔案的位元版本 | .dll 檔案 | 顯示該 DLL 是 32 位元或 64 位元 |
| 解譯單一CS檔案 | 所有檔案 | 透過 Node.js 處理指定的 .cs 檔案 |
| 解譯資料夾內CS檔案 | 資料夾 | 處理資料夾內所有 .cs 檔案（不含子資料夾） |
| 解譯資料夾(含子資料夾)內CS檔案 | 資料夾 | 遞迴處理資料夾及所有子資料夾的 .cs 檔案 |

## 系統需求

- Windows 作業系統
- [Node.js](https://nodejs.org/)（`node` 命令可在 PowerShell 中執行）
- .NET Framework（執行 `.exe` 所需）

## 安裝步驟

1. 編譯專案，取得 `RightClickCopyFileName.exe`
2. 將 `main.js` 放在與 `RightClickCopyFileName.exe` **相同的目錄**下
3. 以**系統管理員權限**執行 `RightClickCopyFileName.exe`（不帶任何參數）
4. 程式會自動將所有右鍵選單項目寫入 Windows Registry

## 使用方式

安裝完成後，直接在檔案總管中對檔案或資料夾按右鍵即可使用。

### 複製檔案名稱
對任意檔案按右鍵 → 選擇「複製檔案名稱」，檔案名稱即複製至剪貼簿。

### 檢查 DLL 位元版本
對 `.dll` 檔案按右鍵 → 選擇「檢查dll檔案的位元版本」，跳出視窗顯示 32 或 64 位元。

### 解譯 CS 檔案
對 `.cs` 檔案按右鍵 → 選擇「解譯單一CS檔案」，程式會：
1. 將原檔案重新命名為 `{檔案名稱}_old.cs`
2. 透過 PowerShell 與 Node.js 建立新的同名 `.cs` 檔案

### 解譯資料夾
對資料夾按右鍵，選擇「解譯資料夾內CS檔案」或「解譯資料夾(含子資料夾)內CS檔案」，批次處理該目錄下的所有 `.cs` 檔案。

## 技術細節

### Registry 路徑

| 功能 | Registry 路徑 |
|---|---|
| 複製檔案名稱 | `*\shell\RightclickCopyFileName` |
| 複製完整名稱 | `*\shell\RightclickCopyFullFileName` |
| 檢查 DLL 位元 | `dllfile\shell\CheckDllFileBase` |
| 解譯單一 CS 檔案 | `*\shell\DecodeCsFile` |
| 解譯資料夾 CS 檔案 | `Directory\shell\DecodeCsFileFolder` |
| 解譯資料夾含子資料夾 | `Directory\shell\DecodeCsFileSubFolder` |

### 命令列參數（main.js）

```
node main.js -f <檔案路徑>              # 處理單一 .cs 檔案
node main.js -s <資料夾路徑>            # 處理資料夾內的 .cs 檔案
node main.js -s <資料夾路徑> -r         # 遞迴處理資料夾及子資料夾
```

### 程式執行流程

1. 使用者在檔案總管對檔案或資料夾按右鍵，選擇功能
2. Windows 透過 Registry 啟動 `RightClickCopyFileName.exe` 並傳入對應參數
3. 程式定位同目錄下的 `main.js`
4. 透過 PowerShell 執行 `node main.js`，並開啟新視窗顯示執行結果

## 注意事項

- 首次安裝需以**系統管理員權限**執行，否則無法寫入 Registry
- `main.js` 必須與 `RightClickCopyFileName.exe` 放在**同一目錄**
- 解譯功能會將原始 `.cs` 檔案重新命名為 `_old.cs`，請確認備份後再使用
