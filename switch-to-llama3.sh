#!/bin/bash
sqlcmd -S '(localdb)\mssqllocaldb' -d startour -Q "UPDATE LlmSettings SET IsActive = 0; UPDATE LlmSettings SET IsActive = 1 WHERE ProviderName = 'Ollama'; SELECT Id, ProviderName, ModelName, IsActive FROM LlmSettings;"
