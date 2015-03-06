#include <metahost.h>
#include <wchar.h>

#pragma comment(lib, "mscoree.lib")

const LPCWSTR DELIM = L";";

__declspec(dllexport) HRESULT ImplantDotNetAssembly(LPWSTR lpArgument)
{
	HRESULT hr;
	ICLRMetaHost *pMetaHost = NULL;
	ICLRRuntimeInfo *pRuntimeInfo = NULL;
	ICLRRuntimeHost *pClrRuntimeHost = NULL;

	// build runtime
	// TODO: add error checking  (pRuntimeInfo->IsLoadable as well)
	hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&pMetaHost);
	hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_ICLRRuntimeInfo, (LPVOID*)&pRuntimeInfo);
	hr = pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&pClrRuntimeHost);

	// Start the runtime
	hr = pClrRuntimeHost->Start();

	// parse the arguments
	LPWSTR pwzTempArgument;
	LPWSTR pwzArgument[4];
	LPWSTR lpwcContext;
	int iCount = 0;

	wprintf(L"Splitting wide string \"%ls\" into tokens:\n", lpArgument);

	pwzTempArgument = wcstok_s(lpArgument, DELIM, &lpwcContext);

	while (pwzTempArgument != NULL) {
		pwzArgument[iCount] = pwzTempArgument;
		wprintf(L"%ls\n", pwzArgument[iCount++]);
		pwzTempArgument = wcstok_s(NULL, DELIM, &lpwcContext);
	}

	DWORD dwReturnValue;
	hr = pClrRuntimeHost->ExecuteInDefaultAppDomain(
		pwzArgument[0],
		pwzArgument[1],
		pwzArgument[2],
		pwzArgument[3],
		&dwReturnValue);

	// We are NOT going to unload the .net runtime
	// hr = pClrRuntimeHost->Stop();

	// Free resources
	pClrRuntimeHost->Release();
	pRuntimeInfo->Release();
	pMetaHost->Release();

	return hr;
}

BOOL APIENTRY DllMain(HMODULE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	// Attemping to load .NET framework inside of DllMain ATTACH can result in a loader lock
	// more info: http://msdn.microsoft.com/en-us/library/ms172219.aspx

	switch (fdwReason)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}

	return TRUE;
}