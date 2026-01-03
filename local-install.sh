dotnet tool uninstall --global Karls.Gitflow.Tool
dotnet pack -p:ToolCommandName=git-flow2 --output ./local-artifacts --configuration Release ./src/Karls.Gitflow.Tool/Karls.Gitflow.Tool.csproj
dotnet tool install --global --add-source ./local-artifacts Karls.Gitflow.Tool
