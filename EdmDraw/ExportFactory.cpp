#include <UnionNXKit\ExportFactory.hpp>
#include <WinKit\WinBase.hpp>

using namespace UnionNXKit;
using namespace WinKit;

ExportFactory::ExportFactory()
{

}

ExportFactory::~ExportFactory()
{

}

void ExportFactory::ExportIges(const char *inputFile,const char *outputFile)
{
	UF_UI_set_status("正在导出IGES.....");

	char *cmdline = (char *)malloc(WIN_COMMAND_LINE_DATA_LEN*sizeof(char));
	sprintf(cmdline,"\"%s\\iges\\iges.exe\" \"%s\" o=\"%s\" d=\"%s\\IGES\\igesexport.def\"",\
		UnSession::ugiiBaseDir,inputFile,outputFile,UnSession::ugiiBaseDir);

	WinBase::CreateUserProcess(cmdline);

	free(cmdline);
}

void ExportFactory::ExportStep214(const char *inputFile,const char *outputFile)
{
	UF_UI_set_status("正在导出STEP214.....");

	CHAR tempEnvironmentVariable[256];
	sprintf(tempEnvironmentVariable,"%s\\STEP214UG\\",UnSession::ugiiBaseDir);
	SetEnvironmentVariableA("ROSE_DB",tempEnvironmentVariable);
	SetEnvironmentVariableA("ROSE",tempEnvironmentVariable);

	char *cmdline = (char *)malloc(WIN_COMMAND_LINE_DATA_LEN*sizeof(char));
	sprintf(cmdline,"\"%s\\step214ug\\step214ug.exe\" \"%s\" o=\"%s\" d=\"%s\\step214ug\\ugstep214.def\"",\
		UnSession::ugiiBaseDir,inputFile,outputFile,UnSession::ugiiBaseDir);

	UnSession::PrintLog(cmdline);

	WinBase::CreateUserProcess(cmdline);

	free(cmdline);
}

void ExportFactory::ExportJT(const char *inputFile,const char *outputFile)
{
	UF_UI_set_status("正在导出JT.....");
	
	char outputDir[_MAX_PATH],_Dir[_MAX_PATH],_Drive[_MAX_DRIVE],_Filename[_MAX_FNAME];
	_splitpath(inputFile,_Drive,_Dir,_Filename,NULL);
	sprintf_s(outputDir,_MAX_PATH,"%s%s",_Drive,_Dir);
	if('\\'==outputDir[strlen(outputDir)-1])
	{
		outputDir[strlen(outputDir)-1]='\0';
	}

	char *cmdline = (char *)malloc(WIN_COMMAND_LINE_DATA_LEN*sizeof(char));
    sprintf(cmdline,"\"%s\\PVTRANS\\ugtopv.exe\" \"%s\" -config=\"%s\\PVTRANS\\tessUG.config\" -force_output_dir=\"%s\"",\
		UnSession::ugiiBaseDir,inputFile,UnSession::ugiiBaseDir,outputDir);

	UnSession::PrintLog(cmdline);

	WinBase::CreateUserProcess(cmdline);

    char tempOutputFile[_MAX_PATH];
	sprintf_s(tempOutputFile,_MAX_PATH,"%s\\%s__MODEL.jt",outputDir,_Filename);

	DeleteFileA(outputFile);
	MoveFile(tempOutputFile,outputFile);

	free(cmdline);
}
