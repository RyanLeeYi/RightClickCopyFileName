const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');
let args = process.argv;
let sourcePathIndex = args.indexOf("-s");
let filePathIndex = args.indexOf("-f");
let recursiveIndex = args.indexOf("-r");
let isRecursive = recursiveIndex !== -1;

// 檢查參數
if (sourcePathIndex === -1 && filePathIndex === -1) {
    console.log("請輸入 -s (資料夾路徑) 或 -f (單一檔案路徑)");
    console.log("用法:");
    console.log("  處理資料夾: node main.js -s C:\\path\\to\\folder");
    console.log("  處理資料夾(遞迴): node main.js -s C:\\path\\to\\folder -r");
    console.log("  處理單一檔案: node main.js -f C:\\path\\to\\file.cs");
    process.exit(1);
}

if (sourcePathIndex !== -1 && filePathIndex !== -1) {
    console.log("錯誤: -s 和 -f 參數不能同時使用，請只選擇其中一個");
    process.exit(1);
}

// 判斷是單一檔案模式還是資料夾模式
const isSingleFileMode = filePathIndex !== -1;
const sourcePath = isSingleFileMode ? args[filePathIndex + 1] : args[sourcePathIndex + 1];

// 單一檔案模式時忽略遞迴參數
if (isSingleFileMode && isRecursive) {
    console.log("注意: 單一檔案模式會忽略 -r 參數");
    isRecursive = false;
}

// 遞迴搜尋所有cs檔案的函數
function findCsFiles(dir, fileList = []) {
    const files = fs.readdirSync(dir);
    
    files.forEach(file => {
        const filePath = path.join(dir, file);
        const stat = fs.statSync(filePath);
        
        if (stat.isDirectory() && isRecursive) {
            // 如果是資料夾且開啟遞迴模式，繼續搜尋
            findCsFiles(filePath, fileList);
        } else if (stat.isFile() && file.endsWith('.cs')) {
            // 如果是cs檔案，加入列表
            fileList.push(filePath);
        }
    });
    
    return fileList;
}

//讀取檔案列表
let files = [];

if (isSingleFileMode) {
    // 單一檔案模式
    if (!fs.existsSync(sourcePath)) {
        console.log(`錯誤: 檔案不存在 - ${sourcePath}`);
        process.exit(1);
    }
    
    if (!sourcePath.endsWith('.cs')) {
        console.log(`錯誤: 指定的檔案不是 .cs 檔案 - ${sourcePath}`);
        process.exit(1);
    }
    
    files = [sourcePath];
    console.log('單一檔案模式');
    console.log(`處理檔案: ${sourcePath}`);
} else {
    // 資料夾模式
    if (!fs.existsSync(sourcePath)) {
        console.log(`錯誤: 資料夾不存在 - ${sourcePath}`);
        process.exit(1);
    }
    
    files = isRecursive ? findCsFiles(sourcePath) : fs.readdirSync(sourcePath).filter(file => file.endsWith('.cs')).map(file => path.join(sourcePath, file));
    console.log(isRecursive ? '遞迴模式：搜尋所有子資料夾' : '一般模式：只搜尋當前資料夾');
    console.log(`找到 ${files.length} 個CS檔案:`, files);
}

if (files.length === 0) {
    console.log('沒有找到任何 .cs 檔案');
    process.exit(0);
}

//讀取並處理每個CS檔案的內容，為每個檔案建立單獨的輸出檔案
files.forEach((filePath, index) => {
    const fileName = path.basename(filePath);
    const fileDir = path.dirname(filePath);
    console.log(`\n正在處理 (${index + 1}/${files.length}): ${filePath}`);
    
    try {
        const content = fs.readFileSync(filePath, 'utf8');
        
        // 先將原檔案重新命名為 {原檔案名稱_old}
        const fileNameWithoutExt = fileName.replace('.cs', '');
        const oldFileName = `${fileNameWithoutExt}_old.cs`;
        const oldFilePath = path.join(fileDir, oldFileName);
        
        // 重新命名原檔案
        fs.renameSync(filePath, oldFilePath);
        console.log(`✓ 原檔案已重新命名為: ${oldFileName}`);
        
        // 最終檔案使用原檔案名稱
        const newFileName = fileName;
        const outputPath = path.join(fileDir, newFileName);
        
        // 直接使用原檔案內容，不加額外資訊
        const fileOutputContent = content;
        
        // 使用PowerShell Write-Output寫入到新檔案，控制命令長度避免太長
        console.log(`正在建立輸出檔案: ${newFileName}`);
        
        try {
            // 將內容分行處理
            const lines = fileOutputContent.split('\n');
            let currentBatch = [];
            let batchIndex = 0;
            
            for (let i = 0; i < lines.length; i++) {
                const line = lines[i];
                // 替換單引號和雙引號
                // 單引號: ' -> ''
                // 雙引號: " -> "^""
                const escapedLine = line.replace(/'/g, "''").replace(/"/g, '"^""');
                
                // 檢查如果加入這一行，命令長度是否會超過5000
                const testBatch = [...currentBatch, escapedLine];
                const testLines = testBatch.map(l => `'${l}'`).join(',');
                const testCommand = `powershell -Command "Write-Output ${testLines} ${batchIndex === 0 ? '>' : '>>'} '${outputPath}'"`;
                
                if (testCommand.length > 5000 && currentBatch.length > 0) {
                    // 執行當前批次
                    const batchLines = currentBatch.map(l => `'${l}'`).join(',');
                    const command = `powershell -Command "Write-Output ${batchLines} ${batchIndex === 0 ? '>' : '>>'} '${outputPath}'"`;
                    
                    // 印出完整命令內容
                    console.log(`\n  ===== 執行命令 (批次 ${batchIndex + 1}, 長度 ${command.length}) =====`);
                    console.log(command);
                    console.log(`  ===== 命令結束 =====\n`);
                    
                    execSync(command, { encoding: 'utf8' });
                    console.log(`  批次 ${batchIndex + 1}: 已寫入 ${currentBatch.length} 行`);
                    
                    // 重置批次
                    currentBatch = [escapedLine];
                    batchIndex++;
                } else {
                    // 加入當前行到批次中
                    currentBatch.push(escapedLine);
                }
            }
            
            // 執行最後一個批次（如果有的話）
            if (currentBatch.length > 0) {
                const batchLines = currentBatch.map(l => `'${l}'`).join(',');
                const command = `powershell -Command "Write-Output ${batchLines} ${batchIndex === 0 ? '>' : '>>'} '${outputPath}'"`;
                
                // 印出完整命令內容
                console.log(`\n  ===== 執行命令 (批次 ${batchIndex + 1}, 長度 ${command.length}) =====`);
                console.log(command);
                console.log(`  ===== 命令結束 =====\n`);
                
                execSync(command, { encoding: 'utf8' });
                console.log(`  批次 ${batchIndex + 1}: 已寫入 ${currentBatch.length} 行`);
            }
            
            console.log(`✓ 已成功建立: ${newFileName} (共 ${batchIndex + 1} 個批次，${lines.length} 行)`);
        } catch (writeErr) {
            console.error(`使用PowerShell Write-Output寫入檔案 ${newFileName} 時發生錯誤:`, writeErr);
        }
        
        // 同時印到console
        console.log(`\n=== ${fileName} 內容 ===`);
        //console.log(content);
        console.log(`=== ${fileName} 結束 ===\n`);
        
    } catch (err) {
        console.error(`無法處理檔案 ${filePath}:`, err);
    }
});

console.log(`\n========================================`);
console.log(`處理完成！共處理 ${files.length} 個檔案`);
console.log(`每個檔案都已處理：`);
console.log(`- {原檔案名稱}_old.cs (原檔案重新命名)`);
console.log(`- {原檔案名稱}.cs (使用PowerShell Write-Output建立的新檔案)`);
console.log(`========================================`);


