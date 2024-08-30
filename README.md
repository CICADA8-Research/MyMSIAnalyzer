# MyMSIAnalyzer
![1_QLgvET0pU3rr2G8VLGtiiQ](https://github.com/user-attachments/assets/f5507deb-a846-4dc0-9b71-f12fb49f727c)


MyMSIAnalyzer is a tool that allows you to detect vulnerabilities inside MSI files. It is able to:
- Check for credential leaks
- Detect vulnerable Custom Actions
- Check MSI files signature (useful for MST Backdoor)
- Check if Custom Actions can be overwritten

In addition, there is a GuiFinder project in the repository. It can be used to detect MSI files that have a graphical interface and run on behalf of the NT AUTHORITY\SYSTEM, allowing you to elevate your privileges via explorer.exe escape

The tool is easy to use:
```shell
.\MyMSIAnalyzer.exe [-path <PATH TO MSI Files. Default value: C:\Windows\Installer>]

.\GuiFinder.exe [--folder <PATH>]
```

If you're not familiar with this method of privilege escalation, I encourage you to read [our article](https://cicada-8.medium.com/evil-msi-a-long-story-about-vulnerabilities-in-msi-files-1a2a1acaf01c).
