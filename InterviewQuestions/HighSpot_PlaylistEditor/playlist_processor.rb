require_relative "services/mixtape_service"
Process.setproctitle("Mixtape Processor")

if(ARGV.count < 2)
  puts 'ERROR: invalid input'
  exit(1)
end

mixtape_service = MixtapeService.new(ARGV[0])
mixtape_service.process_change_set(ARGV[1])
mixtape_service.print_current_state("json/output.json")
