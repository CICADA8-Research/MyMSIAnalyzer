#include <iostream>
#include <locale>
#include <Windows.h>
#include <TlHelp32.h>
#include <string>
#include <vector>
#include <thread>

bool HasGUI(DWORD processID) {
    bool hasGUI = false;
    HWND hwnd = NULL;
    do {
        hwnd = FindWindowEx(NULL, hwnd, NULL, NULL);
        DWORD windowPID = 0;
        GetWindowThreadProcessId(hwnd, &windowPID);
        if (windowPID == processID) {
            hasGUI = true;
            break;
        }
    } while (hwnd != NULL);
    return hasGUI;
}

std::vector<DWORD> GetChildProcesses(DWORD parentPID) {
    std::vector<DWORD> childPIDs;
    HANDLE hSnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnap != INVALID_HANDLE_VALUE) {
        PROCESSENTRY32 pe32 = { sizeof(pe32) };
        if (Process32First(hSnap, &pe32)) {
            do {
                if (pe32.th32ParentProcessID == parentPID) {
                    childPIDs.push_back(pe32.th32ProcessID);
                }
            } while (Process32Next(hSnap, &pe32));
        }
        CloseHandle(hSnap);
    }
    return childPIDs;
}

bool TerminateProcessAndChildren(DWORD pid) {
    std::vector<DWORD> childPIDs = GetChildProcesses(pid);
    for (DWORD childPID : childPIDs) {
        TerminateProcessAndChildren(childPID);
    }
    HANDLE hProcess = OpenProcess(PROCESS_TERMINATE, FALSE, pid);
    if (hProcess == NULL) {
        return false;
    }

    BOOL result = TerminateProcess(hProcess, 0);
    CloseHandle(hProcess);
    return result == TRUE;
}

void GetProcessUsername(DWORD processID, std::wstring& username) {
    HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION, FALSE, processID);
    if (hProcess) {
        HANDLE hToken;
        if (OpenProcessToken(hProcess, TOKEN_QUERY, &hToken)) {
            DWORD len = 0;
            GetTokenInformation(hToken, TokenUser, NULL, 0, &len);
            if (GetLastError() == ERROR_INSUFFICIENT_BUFFER) {
                std::vector<uint8_t> buffer(len);
                if (GetTokenInformation(hToken, TokenUser, buffer.data(), len, &len)) {
                    TOKEN_USER* tokenUser = reinterpret_cast<TOKEN_USER*>(buffer.data());
                    SID_NAME_USE sidUse;
                    DWORD userLen = 0, domainLen = 0;
                    LookupAccountSid(NULL, tokenUser->User.Sid, NULL, &userLen, NULL, &domainLen, &sidUse);

                    std::vector<wchar_t> userName(userLen);
                    std::vector<wchar_t> domainName(domainLen);

                    if (LookupAccountSid(NULL, tokenUser->User.Sid, userName.data(), &userLen, domainName.data(), &domainLen, &sidUse)) {
                        username.assign(userName.data());
                        username = domainName.data() + std::wstring(L"\\") + username;
                    }
                }
            }
            CloseHandle(hToken);
        }
        CloseHandle(hProcess);
    }
}

void ProcessMsiFile(const std::wstring& msiFilePath) {
    std::wstring command = L"msiexec.exe /fa \"" + msiFilePath + L"\"";

    STARTUPINFOW si = { sizeof(si) };
    PROCESS_INFORMATION pi;

    if (CreateProcessW(NULL, &command[0], NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi)) {
        std::wcout << L"File: " << msiFilePath;

        // Wait a bit for process to potentially launch a GUI
        Sleep(2000);

        bool guiFound = HasGUI(pi.dwProcessId);

        std::vector<DWORD> childPIDs = GetChildProcesses(pi.dwProcessId);
        for (DWORD childPID : childPIDs) {
            if (HasGUI(childPID)) {
                guiFound = true;
                break;
            }
        }

        if (guiFound) {
            std::wcout << L" [+] HAS GUI";
        }
        else {
            std::wcout << L" [-] NO GUI ";
        }

        std::wstring username;
        GetProcessUsername(pi.dwProcessId, username);
        std::wcout << L" [?] Running from: " << username << std::endl;

        // Terminate the process and the child processes
        TerminateProcessAndChildren(pi.dwProcessId);
        WaitForSingleObject(pi.hProcess, INFINITE);

        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
    }
    else {
        std::wcerr << L"Cant run process: " << msiFilePath << std::endl;
    }
}

void ScanDirectory(const std::wstring& directory) {
    WIN32_FIND_DATA findFileData;
    HANDLE hFind = FindFirstFile((directory + L"\\*").c_str(), &findFileData);

    if (hFind == INVALID_HANDLE_VALUE) {
        std::wcerr << L"Cant open directory: " << directory << std::endl;
        return;
    }

    do {
        const std::wstring fileOrDirName = findFileData.cFileName;

        if (fileOrDirName == L"." || fileOrDirName == L"..") {
            continue;
        }

        std::wstring fullPath = directory + L"\\" + fileOrDirName;

        if (findFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) {
            ScanDirectory(fullPath);
        } else {
            if (fullPath.substr(fullPath.find_last_of(L".") + 1) == L"msi") {
                ProcessMsiFile(fullPath);
            }
        }
    } while (FindNextFile(hFind, &findFileData) != 0); FindClose(hFind);
}

int wmain(int argc, wchar_t* argv[]) {
    setlocale(LC_ALL, "");

    std::wstring directoryToScan = L"C:\\"; // Default directory. U can change to C:\\Windows\\Installer

    for (int i = 1; i < argc; ++i) {
        if (std::wstring(argv[i]) == L"--folder" && i + 1 < argc) {
            directoryToScan = argv[i + 1];
            break;
        }
    }

    std::wcout << L"Recursive Scanning Directory: " << directoryToScan << std::endl;
    ScanDirectory(directoryToScan);

    return 0;
}